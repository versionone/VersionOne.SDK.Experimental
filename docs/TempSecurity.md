# Identity and Access Management in VersionOne

There are four ways that the VersionOne software can identify (authenticate) the Member who is initiating web requests to the application. There is one mechanism by which an identified Member is granted access (authorized) to perform actions within the system. This document outlines these mechanisms and summarizes the relevant source code and implementation details.

## Identity (Authentication) Methods
VersionOne’s four supported identity (authentication) methods are outlined in the Authentication page in the VersionOne Help Center. The methods are VersionOne Authentication, Windows Integrated Authentication, On-Demand Single Sign-On Authentication, and On-Premise Single Sign-On Authentication.

We’re going to look at how each of them is supported in the code, including the different implementation details that make them work for the following different classes of server side resources that comprise the VersionOne application:

* Legacy ASP.NET Web Forms pages
 * And the foundational `VersionOne.Web.Security` static class subsystem that facilities this and all the rest
* HTTP Handlers (API endpoints, and standalone implementations of IHttpHandler)
* IceNine (System.Web.Mvc.Controller-derived MVC controllers)
* Nuances for AJAX scenarios for each of the three classes above

### Legacy ASP.NET Web Forms pages

The oldest type of server side resources in the VersionOne application are Legacy ASP.NET Web Form pages. The class [`VersionOne.Web.DeafultPage`](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Default.aspx.cs#L27) in the file [`Core.VersionOne.Web/Default.aspx.cs`](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Default.aspx.cs) calls [`VersionOne.Web.Security.Login()`](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Util/Security/Security.cs#L121) in its `OnInit` method.

#### VersionOne.Web.Security.Login() implementation

```csharp
public static void Login()
{
  try { Global.Login(Ticket); }
  catch (SecurityException) { Logout(); }
}
```
#### Security.Login() Dependent Types Graph

There are a number of peers to DefaultPage that also call `Security.Login()`. Here is an NDepend graph showing all types that directly
depend upon `Security.Login()`:

![Securiy.Login() Dependent Types](http://img607.imageshack.us/img607/7564/z5pq.png)
##### Code Query Language for this data

```cql
from t in Types where t.IsUsing ("VersionOne.Web.Security.Login()")
select new { t, t.NbILInstructions }
```

#### Security.Login() Method Dependencies Graph

This one method abstracts a lot of complex logic. Here's a bird's eye view generated by NDepend of all methods (which includes getters and setters) that it has a static dependency on:

![VersionOne.Web.Security.Login() Dependency Graph](http://img96.imageshack.us/img96/6186/bi6g.png)

##### Code Query Language statement to get this information in NDepend

```cql
from t in Types 
let depth0 = t.DepthOfIsUsedBy("VersionOne.Web.Security.Login()")
where depth0  >= 0 orderby depth0
select new { t, depth0 }
```

#### Security.Login() Type Dependencies

Now, this may be too much information to grasp in one shot. Here's a view limited to just the types that `Security.Login()` depends on:

![Security.Login() Type Dependencies](http://img35.imageshack.us/img35/2830/orkg.png)

### Tour of Security.Login() in VersionOne Authentication Mode

That's still too much information to understand anything. So, let's take a tour of this method's execution when VersionOne 
is configured in VersionOne Authentication Mode.

We start with the same starting point code from above:

```csharp
public static void Login()
{
  try { Global.Login(Ticket); }
  catch (SecurityException) { Logout(); }
}
```

Since we immediately delegate out to `Global.Login(Ticket)`, let's see how the static `Ticket` works:


#### [VersionOne.Web.Security.Ticket](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Util/Security/Security.cs#L304) static property 

```csharp
public static Ticket Ticket
{
  get
	{
		var items = HttpContext.Current.Items;
		if (!items.Contains("Ticket"))
			Authenticate();
		return (Ticket)items["Ticket"];
	}
}
```

* The dictionary `HttpContext.Current.Items` contains data for the lifetime of a single request. If a `Ticket` already exists
then we cast it to the right class and return it.
* Otherwise, it's off to `Authenticate()`

#### [Security.Authenticate()](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Util/Security/Security.cs#L139) Implementation

```csharp
private static void Authenticate()
{
	Ticket ticket = GetRequestTicket();

	if (ticket == null)
	{
		ticket = AuthenticateByHeaders() ??
			(IsSingleSignOn ?
				AuthenticateBySSO() :
				AuthenticateByBasicAuth());

		WriteTicketToCookie(ticket, false);
	}

	SetTicket(ticket);
}
```
We will only look at `GetRequestTicket()` and `SaveTicket(ticket)` now, because the first is really the default **VersionOne Authentication Mode** implementation.

#### [Security.GetRequestTicket()](https://github.com/versionone/Core/blob/developing/VersionOne.Web/Util/Security/Security.cs#L156) Implementation

```
private static Ticket GetRequestTicket()
{
	string cookieTicketText = GetCookieTicketText();
	string urlTicketText = GetQueryStringTicketText();

	Ticket ticket = DeserializeTicket(urlTicketText ?? cookieTicketText);

	if (ticket != null && !IsLoginAllowed(ticket.MemberID))
	{
		ticket = null;
		WriteTicketTextToCookie(null, DoesntExpire);
	}

// The code to force the query string ticket into a cookie allowed PDF generation to work for pages with AJAX
// content that requires authentication, such as My Dashboard. In that scenario, PDF generator would request the
// page with ticket in the URL, and subsequent AJAX requests would rely on the ticket being moved to the cookie.
// However, this introduced a security hole when users would share their RSS feed URLs with others, thus allowing
// them to impersonate the subscriber of that feed (S-17804).
// It is now believed that all pages automatically inline all AJAX content (e.g. gadgets and graphs) during PDF
// generation, so moving the ticket from query string into a cookie is no longer necessary.
#if false
	if (ticket != null && urlTicketText != null && cookieTicketText != urlTicketText)
		WriteTicketTextToCookie(urlTicketText, DoesntExpire);
#endif

	return ticket;
}
```


