# VersionOne Web API Quick Start

Getting started with the VersionOne Web API is easy.
This article walks you through hands-on examples of creating a new project and adding stories to it with simple tools like cURL and JavaScript with the popular jQuery library.

# Tasks we'll cover

Here's what we'll do:

1. Query the admin user's details and get the results as JSON, XML, or YAML using the cURL command line tool
2. Query the admin user's details and get the results as JSON, XML, or YAML using the JavaScript jQuery library
2. Create a new Project scope using HTML 5 and jQuery
3. Add stories to the project using HTML 5 and jQuery
4. View existing stories from the project using HTML 5 and jQuery
5. See examples of doing similar tasks with Python, C#, and Java

# The simplest thing that could possibly work

*"What's possible? What is the simplest thing we could say in code, so that we'll be talking about something that's on the screen, instead of something that's ill-formed in our mind."*
-- [Ward Cunningham](http://www.artima.com/intv/simplest3.html), inventor of Wiki technology and an original Agile Manifesto signatory.

As an agile developer, you probably recognize that quote, or at least the sentiment. So, let's get started with the first task in programming with VersionOne.

We're using the public VersionOne API Developer Playground server, but you can adapt these scripts to run against your own instance if you have one.

## Task 1: Query the admin user's details using cURL or jQuery

 To get data out of the VersionOne Web API with the least number of lines of code possible, 
 you can use the [command-line tool cURL](http://curl.haxx.se/):
 

### cURL Code

```batch
curl --basic -u admin:admin http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?acceptFormat=haljson
```

### jQuery Code



### Output

```json
{
  "SendConversationEmails": "true",
  "AssetState": "64",
  "Email": "admin@company.com",
  "Nickname": "Admin",
  "Followers.Nickname": "",
  "SecurityScope.Name": "",
  "NotifyViaEmail": "false",
  "IsLoginDisabled": "false",
  "Description": "",
  "AssetType": "Member",
  "Phone": "777-666-888-99",
  "Followers.Name": "",
  "DefaultRole.Name": "Role.Name'System Admin",
  "Username": "admin",
  "UsesLicense": "false",
  "Ideas": "",
  "Name": "J",
  "_links": {
    "self": {
      "href": "/VersionOne.Web/rest-1.v1/Data/Member/20",
      "id": "Member:20"
    },
    "DefaultRole": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Role/1",
        "idref": "Role:1"
      }
    ],
    "SecurityScope": [],
    "Avatar": [],
    "Followers": []
  }
}
```

### Explanation

The cURL tool supports lots of options for interacting with HTTP servers and APIs. We only needed to use the `--basic -u admin:admin` flags to pass the credentials to the server.




    </p>
<p>    
Well, suppose you want to write an app that lets you create stories everytime you think of one, or any time someone in your organization comes to you and says, "Hey you, Programmer-Automater-Ninja-Wizard-Guy-or-Gal, wouldn't it be great if As A User When ... I could ... So That ..." -- you could just whip off some command-line jujitsu and then say, "Your story is my command, and by the way, I already added it to the backlog in no time!"
    
    </p>
    
    <h1>We've Got You Covered</h1>
 
    <h2>The Least Code</h2>

    To get data out of the VersionOne Web API with the least number of lines of code possible, you can use the command-line tool cURL. Here's how to see the default administrator user's details in VersionOne with cURL from the command line:

    curl --basic -u admin:admin http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?acceptFormat=haljson

    That's it. You'll get back something like this:

{
  "SendConversationEmails": "true",
  "AssetState": "64",
  "Email": "admin@company.com",
  "Nickname": "Admin",
  "Followers.Nickname": "",
  "SecurityScope.Name": "",
  "NotifyViaEmail": "false",
  "IsLoginDisabled": "false",
  "Description": "",
  "AssetType": "Member",
  "Phone": "777-666-888-99",
  "Followers.Name": "",
  "DefaultRole.Name": "Role.Name'System Admin",
  "Username": "admin",
  "UsesLicense": "false",
  "Ideas": "",
  "Name": "J",
  "_links": {
    "self": {
      "href": "/VersionOne.Web/rest-1.v1/Data/Member/20",
      "id": "Member:20"
    },
    "DefaultRole": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Role/1",
        "idref": "Role:1"
      }
    ],
    "SecurityScope": [],
    "Avatar": [],
    "Followers": []
  }
}




<script type="text/javascript">
$(function() {
    var contentType = "hal+json";
    
    var credentialHeaders = { 
        "Authorization": "Basic " + btoa("admin:admin") 
    };
    
    var serviceUrl = "http://localhost/VersionOne.Web/rest-1.v1/Data/Story?" + $.param({acceptFormat:contentType});
    
    var newStory = {
        Name : "Simplest Story",
        Description: "Hello Storyworld!",
        _links: {
            Scope: { idref: "Scope:0" }               
        }
    };  
    
    $.ajax({
        url: serviceUrl,
        headers: credentialHeaders,
        type: "POST",
        contentType: contentType,
        data: JSON.stringify(newStory)
    }).done(function (data) {  
        console.log(data);
    });      
});​
</script>
    <hr/>    
<p>
    <i>"What's possible? What is the simplest thing we could say in code, so that we'll be talking about something that's on the screen, instead of something that's ill-formed in our mind."</i> -- Ward Cunningham, inventor of Wiki technology and an original Agile Manifesto signatory (source: <a href="http://www.artima.com/intv/simplest3.html">Artima interview</a>)
    </p>
<p>    
If you want to start programming against the VersionOne Web API, you may have asked that same famous question as the grat Agile forefathers: "What is the simplest thing that could possibly work?"
    </p>
<p>    
Well, suppose you want to write an app that lets you create stories everytime you think of one, or any time someone in your organization comes to you and says, "Hey you, Programmer-Automater-Ninja-Wizard-Guy-or-Gal, wouldn't it be great if As A User When ... I could ... So That ..." -- you could just whip off some command-line jujitsu and then say, "Your story is my command, and by the way, I already added it to the backlog in no time!"
    
    </p>
    
    <h1>We've Got You Covered</h1>
 
    <h2>The Least Code</h2>

    To get data out of the VersionOne Web API with the least number of lines of code possible, you can use the command-line tool cURL. Here's how to see the default administrator user's details in VersionOne with cURL from the command line:

    curl --basic -u admin:admin http://localhost/VersionOne.Web/rest-1.v1/Data/Member/20?acceptFormat=haljson

    That's it. You'll get back something like this:

{
  "SendConversationEmails": "true",
  "AssetState": "64",
  "Email": "admin@company.com",
  "Nickname": "Admin",
  "Followers.Nickname": "",
  "SecurityScope.Name": "",
  "NotifyViaEmail": "false",
  "IsLoginDisabled": "false",
  "Description": "",
  "AssetType": "Member",
  "Phone": "777-666-888-99",
  "Followers.Name": "",
  "DefaultRole.Name": "Role.Name'System Admin",
  "Username": "admin",
  "UsesLicense": "false",
  "Ideas": "",
  "Name": "J",
  "_links": {
    "self": {
      "href": "/VersionOne.Web/rest-1.v1/Data/Member/20",
      "id": "Member:20"
    },
    "DefaultRole": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Role/1",
        "idref": "Role:1"
      }
    ],
    "SecurityScope": [],
    "Avatar": [],
    "Followers": []
  }
}

    Ok, great, but what about stories? VersionOne is all about planning and tracking stories from idea to done!

    Well, slow down. Let's start with looking at the master project, the System (All Projects):

    curl --basic -u admin:admin http://localhost/VersionOne.Web/rest-1.v1/Data/Scope/0?acceptFormat=haljson

    You'll get back a big bunch of properties:

{
  "Status.Name": "",
  "Owner.Name": "J",
  "Owner.Nickname": "Admin",
  "AssetType": "Scope",
  "TestSuite.Name": "",
  "SecurityScope.Name": "System (All Projects)",
  "BuildProjects.Name": "",
  "EndDate": "",
  "Reference": "",
  "Description": "",
  "AssetState": "64",
  "Parent.Name": "",
  "Name": "System (All Projects)",
  "Scheme.Name": "Default Scheme",
  "BeginDate": "2007-09-08",
  "Schedule.Name": "System (All Projects) Schedule",
  "Ideas": "",
  "_links": {
    "self": {
      "href": "/VersionOne.Web/rest-1.v1/Data/Scope/0",
      "id": "Scope:0"
    },
    "Parent": [],
    "Schedule": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Schedule/1792",
        "idref": "Schedule:1792"
      }
    ],
    "Scheme": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Scheme/1797",
        "idref": "Scheme:1797"
      }
    ],
    "BuildProjects": [],
    "SecurityScope": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Scope/0",
        "idref": "Scope:0"
      }
    ],
    "Owner": [
      {
        "href": "/VersionOne.Web/rest-1.v1/Data/Member/20",
        "idref": "Member:20"
      }
    ],
    "Status": [],
    "TestSuite": []
  }
}

    Fine, now can we make a story, please?

    Take now that we now know that the project has an "Oid", or Object Id, of "Scope:0", by looking at the _links { self ... } reference. We need it next.

    How to create a new story with cURL:

    curl --basic -u admin:admin ^
-H "Content-Type:haljson" ^
-X POST http://localhost/VersionOne.Web/rest-1.v1/Data/Story?acceptFormat=haljson ^
--data "{'Name':'Simple Story','Description':'Hello Storyworld!',_links:{'Scope':{'idref':'Scope:0'}}}"

    Ok, that's easy, but how a web page that lets me or another user enter the name, description, and a quick estimate?

    Sounds good too me. Here's a complete working example that let's you do that, using only the jQuery open-source JavaScript library:
    
    <ul>
        <li>Creates a new story, and </li>
        <li>Displays the story's newly-created ID</li>
    </ul>        
    
    <div style="background-color:lightgray;padding:3px;border:1px solid #777777">
    <form action="#">        
        <div>
            <b>Story Title:</b><br/>
            <input type="text" id="Name" value="Story Title" style="width:300px" />
        </div>
        <div>
            <b>Story Description:</b><br/>
            <textarea id="Description" style="height:100px;width:300px;"></textarea>
        </div>
        <div>
            <b>Estimate:</b><br/>
            <input type="text" id="Estimate" style="width:300px;" />
        </div>        
        <button id="createStory">Create Story</button>
    </form>
        
    </div>​


    That's pretty cool, but how about something more complete and useful? How about a mobile-app that lets people quickly create stories and estimates, but also search or browse the existing stories so they don't create ones that have already been done?

    Now we're talking. This will put together all the concepts in the VersionOne API:

    Querying, Adding new Assets, Updating existing assets, and Deleting assets.

    Let's go!

