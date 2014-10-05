# Dialogue For Umbraco #

Dialogue is a forum/bulletin board Umbraco 7.1 upwards. It is a semi port of [MVCForum](http://www.mvcforum.com) and has some features similar to StackOverFlow.

It's built to use Umbraco API's as much as possible, but also relies on EntityFramework v6.1 for dealing with the main forum. It has only been tested with SQLExpress.

**Current Features Include**

- Multi-Lingual / Localisation (Using Umbraco)
- Points System
- Moderate Topics & Posts
- Badge System (Like StackOverflow)
- Permission System
- Roles / Member Groups (Using Umbraco)
- Mark Posts As Solution
- Vote Up / Down Posts
- Global and Weekly points Leader board
- Responsive Bootstrap Theme
- Latest Activity
- Simple API / ServiceFactory
- Polls
- Spam Prevention
- Facebook & Google Login
- Private Messages
- Member & Post Reporting
- Favourite Posts
- Plus loads more!!

We are always looking for feedback on improvements, bugs and new features so please give it a spin and let us know what you think. Dialogue is designed and developed by the team at [Aptitude](http://www.aptitude.co.uk)

## Video ##

I did a screenr a little while back, which gives a very quick overview of the package.

[https://www.screenr.com/Zy0N](https://www.screenr.com/Zy0N)

## Installation ##

Just download the package from the downloads section, and install into Umbraco as normal making sure you click the 'Complete Installation' button on the 'Almost Done' page.

**IMPORTANT**: Make sure you restart the website (Recycle the app pool) after successful installation. This is to make sure the AppStart kicks in and the custom routes in Dialogue are registered. I'm looking into this, and hopefully resolve this in v1 release.

## Forum Setup ##

Once you have completed the install, you will need to publish the forum and categories (And login and register pages if using them). Permissions tab won't work on a Category unless it's published.

To be able to administrate the forum fully, you will need to register a member on the forum (Creates an Umbraco member) and then in the backoffice UI find that member in the members section and be sure to put that member into the ‘**Dialogue Admin**’ role. 

Also, make sure you tick the ‘**Can Edit Other Members**’ property in the ‘Settings’ tab for the member. This member is now a full Admin and can manage the forum and others members fully.

Also, don't forget to set up your Category Permissions (On every category node) for ‘Dialogue Standard’ (*The default starting group for new members*), or new signups to your forum won’t have permission to do anything.

Have a look through all the tabs on the forum root, and edit the settings as needed. Most options has descriptions explaining what they do.

**PLEASE NOTE** DO NOT change the names of the default roles/groups. You will break the forum.

## Integration ##

You should be able to integrate Dialogue into your own sites, you just need to copy the 'Default' theme folder (Including all sub folders and files) and rename it to whatever you want.

Then in the backoffice, change the 'Theme' to be your new folder. Now you can start updating the styles as you want.

Make sure you don't remove this JavaScript entry, as all the Ajax calls depend on it.

    var app_base = '@Url.Content("~/")';

The Theme engine is more or less the same as [Shannons Articulate blog](https://github.com/Shandem/Articulate) (*As that's where the code was lifted from*). So have a look at his docs for more information too.

[https://github.com/Shandem/Articulate/wiki/Themes](https://github.com/Shandem/Articulate/wiki/Themes)

## Extending / Source ##

The source code in this repo is the full Umbraco site with Database backup that I use to develop Dialogue with. To use it yourself, you should be able to just unzip/clone and setup like a normal Umbraco site then restore the Database from the .bak file in the 'Database' solution folder.

Fire it up and login to the backoffice using 

    admin  
    password

All members registered have a password of Testing too. Or just register your own member.

## ServiceFactory / UnitOfWork ##

There is a ServiceFactory that you should be able to use anywhere in your site after installation, so you can query data from the forum and use it as you wish.

    ServiceFactory.PostService.Whatever...  
    ServiceFactory.TopicService.Whatever...

etc...

If you want to make updates, creates or deletes to any data that is powered via EntityFramework then you need to use the UnitOfWork Manager and follow our convention. Example of using it below

    using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
    {
	    try
	    {
		    // Do all logic here
		    
		    // Commit the transaction
		    unitOfWork.Commit();
	    }
	    catch (Exception ex)
	    {
		    // Roll back database changes 
		    unitOfWork.Rollback();
		    // Log the error
		    LogError(ex);
		    
		    // Do what you want
	    }
    }

`UnitOfWorkManager` shown above is a property on our BaseController, but you can create a `UnitOfWorkManager` by doing the following

    var UnitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);

Have a look in the Controllers for more examples of how to use it.

## Permissions ##

Permissions in Dialogue are based on the Users Role/Group and the Category they are currently in (If in a Topic, the parent Category to the topic).

All permissions can be queried via the PermissionService which you can get via the ServiceFactory ie.

    ServiceFactory.PermissionService.WhatEver()

To check permissions for a member it's pretty simple. You just pass in the Category and the Member Role to the method below and you get a permissionSet back (*Which is just a dictionary*)

    var permissions = ServiceFactory.PermissionService.GetPermissions(Category, MembersGroup);

Then to check a permission you just look it up by name and check `.IsTicked`. We store all the shipped permissions in the `AppConstant` class.

    if (permissions[AppConstants.PermissionDenyAccess].IsTicked)
    {
    	// Do Whatever
    }

There are also some property based permissions at a member level (Such as disable posting, Private Messages etc...), which you can edit via the members section on the member itself.

###Adding a new permission###

This is pretty simple too. Just create a constant with the name of the permission. For example, if I wanted to add a permission called `Can See Unicorns` I'd just create a constant like so.

    public const string PermissionCanSeeUnicorns = "Can See Unicorns";

Now in the `DialoguePermission` database table I need to add an entry in for this permission. I just [generate a Guid for the ID](http://www.webdesigncompany.co.uk/comb-guid/) (*Preferably a Comb Guid as per link to help with SQL performance*) and in the Name field use the string name. i.e.

> Id = 7d1bda05-e36b-40f7-a857-a3b300f64696  
> Name = Can See Unicorns

That's it. You will now be able to select this permission on Categories, and when using `GetPermissions()` you could do

    permissions[MyConstants.PermissionCanSeeUnicorns].IsTicked

To see if this new permission has been enabled on the Category.

## Vitrual Nodes ##

Dialogue only comes with about 4 DocTypes. The rest of the pages are done using `DialogueVirtualPage` which uses the base class `PublishedContentWrapped`. This means that all the pages that are not powered by DocTypes are powered via this.

This is taken from Shannons Articulate package again, so for more information about it have a look through his docs.

But in short, it means we can have pages without having nodes. And we can use all the normal Umbraco utilities such as CurrentPage. If you use CurrentPage on a virtual node, you get the ForumRoot back. 

All the 'pages' (`By that I mean not Topics or member profiles`) can be found in the  `DialoguePageController`. So have a look through it, and also see the `Urls` class with the `UrlType` enum to get Urls for these pages.

When using forms, and posting data we then posting to a normal MVC controller and redirect back to the virtual node as needed.

**NOTE:** These virtual nodes/routes are added at AppStartUp, so after installation the website must be restarted to allow this to kick in and add the routes or you'll just get 404 pages.

## Badges ##

Badges are awarded to forum users when certain milestones are achieved. For example, there is a simple badge awarded on the first anniversary of a user's registration. Dialogue ships with several badge examples (*These can be found in the Badges Solution folder*).

The badge system is highly configurable allowing you to create your own badges. To understand how the badge system works, you need to understand the difference between badge types and badge instances.

Dialogue understands a set of badge types. Badge types relate to specific events that might occur when users are active in the forum.  For example, one badge type is triggered when a user up-votes a post. 

When the up-vote occurs Dialogue examines all the badges of type "up-vote" to see if any apply to the specified user. Dialogue ships with two instances of the up-vote badge type: a badge awarded to post authors the first time one of their posts receives an up-vote, and another badge awarded to users after they give their first up-vote.

This means you can have many badges of the same type. It is easy to imagine many instances of the up-vote badge type: an instance awarded when you make 100 up-votes, an instance awarded when you receive 30 up-votes in a month, and so on.

Dialogue allows you to create new instances of existing badge types, and also to define your own badge types. These are all done via Classes, so you can compile them and just pop them in the bin folder. This also makes them easy to share with other people.

### Creating an Instance of a Badge Type ###

**Step 1: Create a Class**

Badge instances equate to classes, so create a new class:

    public class MyBadgeInstance {
    }

You can create this in any assembly. You will need to reference the Dialogue dll named "Dialogue.Logic.dll".

**Step 2: Inherit from the Badge Type Interface**

Each badge type in the system is defined by an interface. For example, the up-vote badge type is defined by the interface `IVoteUpBadge`,  which is part of the "Dialogue.Logic.Interfaces.Badges" namespace.

To make a new instance of a badge type you make a class that inherits from the required badge type interface. For example:

    public class MyBadgeInstance : IVoteUpBadge {
    }

**Step 3: Implement the Rule Method**

Badges are awarded according to rules. When you create a badge instance, you must supply the rule. Each badge interface will require you to implement the method `Rule`, typically as follows:

    public class MyBadgeInstance : IVoteUpBadge {
	    public bool Rule(Member user)
	    {
	    	return false;
	    }
    }

The rule method should use the `ServiceFactory` parameter to determine whether the user passed as a parameter should be awarded the badge, and return true if the badge should be awarded. For example, the rule might calculate whether the user has sufficient points to acquire the badge.

You can assume that the user has been updated for the related activity BEFORE the rule is called. For example, if the badge is for a certain threshold of votes then you can assume that the triggering vote has already been applied to the user. 

You can also assume that Dialogue has determined whether or not the user has the badge before the rule is called. In other words by default badges are not awarded twice to the same user.

**Step 4: Decorate the Class with Attributes**

Dialogue analyses the badge classes using reflection, and it expects some attributes to be present on each class, as follows:

**Id**: specifies a Guid value that uniquely identifies your badge instance  
**Name**: a meaningful name for your badge  
**DisplayName**: a name used in the web pages for your badge   
**Description**: some friendly text describing your badge  
**Image**: the name of an image file that will be displayed as the badge   
(Found in the Dialogue plugin folder Dialogue > Content > Images > badges)  
**AwardsPoints**: The amount of points awarded for getting this badge

For example, here is the complete UserVoteUpBadge

    [Id("c9913ee2-b8e0-4543-8930-c723497ee65c")]
    [Name("UserVoteUp")]
    [DisplayName("You've Given Your First Vote Up")]
    [Description("This badge is awarded to users after they make their first vote up.")]
    [Image("UserVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class UserVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(Member user)
        {
            var userVotes = ServiceFactory.VoteService.GetAllVotesByUser(user.Id).ToList();
            return userVotes.Count >= 1;
        }
    }

**Step 5: Build and Copy the Assembly**

Compile the assembly that contains your new badge class. Drop the DLL into the Umbraco  bin folder. Dialogue will detect and load the new badge at application start up.

###Creating a New Badge Type###

**Step 1: Create a New Badge Interface**

In this example we will create a new badge type that is awarded when a post is down-voted. "DownVote" will be our new badge type, and you can imagine instances such as "Sceptical" awarded to users who do a lot of down-voting.

Each badge type is defined by a specific interface that you create. The interface classes live in the "Dialogue.Logic" assembly, in the "Interfaces/Badges" folder.

Your new interface MUST inherit from the IBadge interface:

    public interface IVoteDownBadge : IBadge {    
    }

Your interface is complete.

**Step 2: Define the Badge Type and Qualified Name in Code**

So that Dialogue can manage the badges at run time you need to declare some values in the "Badge" domain class. This class is contained in "Badge.cs" within the "Dialogue.Logic" assembly in the folder "Models".

Add an enumeration value for your new badge type (*VoteDown*):
 
    public enum BadgeType {
	    VoteUp,
	    MarkAsSolution,
	    Time,
	    VoteDown
    }
 
Map the interface's fully qualified name to the new badge type (*See last entry*): 

    public static readonly Dictionary<BadgeType, string> BadgeClassNames = new Dictionary<BadgeType, string>
    {
	    {BadgeType.VoteUp, "Dialogue.Logic.Interfaces.Badges.IVoteUpBadge"},
	    {BadgeType.MarkAsSolution, "Dialogue.Logic.Interfaces.Badges.IMarkAsSolutionBadge"},
	    {BadgeType.Time, "Dialogue.Logic.Interfaces.Badges.ITimeBadge"},
	    {BadgeType.VoteDown, "Dialogue.Logic.Interfaces.Badges.IVoteDownBadge"}
    };
 

**You only need to do steps 1 and 2 to define a new badge type. The remaining steps are concerned with using the new type.**

**Step 3: Modify a View and a Controller to Process Your New Badges**

The class "BadgeController" handles badge update requests. Make a new action in this controller to trigger processing for your new badge type, for example:
    
    [HttpPost]
    public void VoteDownPost(VoteDownBadgeViewModel voteDownBadgeViewModel)
    {
	    using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
	    {
		    try {
			    var post = ServiceFactory.PostService.Get(voteDownBadgeViewModel.PostId);
			    var currentUser = ServiceFactory.MemberService.Get(User.Identity.Name);			    
			    var badgeAwarded = ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteDown, currentUser) | ServiceFactory.BadgeService.ProcessBadge(BadgeType.VoteDown, post.User);
			    
			    if (badgeAwarded)
			    {
			    	unitOfwork.Commit();
			    }
		    }
		    catch (Exception ex)
		    {
			    unitOfwork.Rollback();
			    LoggingService.Error(ex);
		    }
	    }
    }

In this action, a view model containing the post id of the down-voted post is passed. The action then retrieves the logged on user, plus the owner of the post. It then calls for badge processing using the down-vote badge type. It does this twice: once for the user making the vote down (current user) and then for the user owning the post.

This example code implies the existence of two instances of the down-vote badge: one awarded to the user making the down-vote and one awarded to the user receiving the down-vote. These badges are different, but they are the same type of badge.

Typically the controller actions that call for badge processing are themselves called via Ajax when an activity takes place in the forum, in this case a user down-voting a post.

## Social Logins ##

Our Social logins (Currently only Facebook and Google) is powered via a fantastic library called [Skybrud.Social](http://social.skybrud.dk/) - To enable social logins you need to add the appropriate keys into the Social settings on your forum root. See below.

**Facebook uses OAuth 2.0** for authentication and communication. In order for
users to authenticate with the Facebook API, you must specify the ID, secret and redirect URI of your Facebook app. You can create a new app at the following URL:
[https://developers.facebook.com/](https://developers.facebook.com/)

**Google uses OAuth 2.0** for authentication and communication. In order for
users to authenticate with the Google API, you must specify the ID, secret and redirect URI of your Google app. You can create a new app at the following URL:
[https://console.developers.google.com/project](https://console.developers.google.com/project)

Notice: When a user is redirected to the Google of Facebook login dialog, the scope (permissions of the app) are specified. You can change these if you require further permissions. Just look in the OAuthControllers folder and edit the controller as needed.

## Swapping Out The Markdown Editor ##

Dialogue comes with 2 Rich Text Editors build in. MarkDown Editor (Default) and TinyMCE. These can be found in the following folder in your Umbraco installation (Read here to find out more about EditorTemplates)

Views > Shared > EditorTemplates

To swap out the markdown editor with the TinyMCE Editor you just need to search for the following attribute in a few of the ViewModels.

    [UIHint(AppConstants.EditorType), AllowHtml]

And then just change it to whatever editor you want to use, by adding using the view name of the editor without the .cshtml - We have a constant for this, and you can just uncomment the editor you want.

    //public const string EditorType = "tinymceeditor";
    public const string EditorType = "markdowneditor";

To use TinyMCE, comment out the markdown one and uncomment the tinymce one. If you wanted to change it manually, you would change the attribute to.

    [UIHint("tinymceeditor"), AllowHtml]

> NOTE: Also remove you can remove the AllowHtml parameter if you don't want to allow HTML to be entered into your editor. i.e.

    [UIHint("markdowneditor")]

###Create Your Own###

If there is another Editor you want to use, its very simple to add it in. Just make a new view in the `EditorTemplates` folder and name it whatever the editor is. Then as above just change the UIHint to your `EditorTemplates` name without the .cshtml at the end.

## Embed Videos ##

You can embed YouTube, Vimeo and Screenr videos into your posts, just paste the full link to the video in and the videos will be auto updated to the embed code.

## Upgrading ##

You should be able to just re-install the package over the top of the previous one. We are trying to make sure the installer takes care of everything for you.