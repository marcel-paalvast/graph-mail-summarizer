[![Hack Together: Microsoft Graph and .NET](https://img.shields.io/badge/Microsoft%20-Hack--Together-orange?style=for-the-badge&logo=microsoft)](https://github.com/microsoft/hack-together)

# Graph Mail Summarizer

Mail Summarizer is a web application that helps you manage your emails more efficiently by generating brief summaries of their contents. Save yourself time by getting a consise and organized overview of all the mails you might have missed while you were away!

Using the application is easy. Simple connect to your email account you want summarized. Select the start and end date to filter a date range and let the app analyze your mailbox. Within a moment you'll receive a summary that captures the key information and main ideas in your inbox.

https://user-images.githubusercontent.com/126659601/225466703-5882a50a-8e9e-4bf3-9872-d1c49939e191.mp4

## Key Features

The application features a single page application (SPA) that makes a request to a backend api. The api is setup as durable function app that leverages the jwt token retrieved in the request to retrieve your mails using the Azure on-behalf-of authentication flow.

The mails are individually fed to OpenAi to create detailed summaries and a summary of all summeries combined. The result is then converted into a html template that is send as mail to requesters mail address obtained from the jwt token.

## How to setup your own environment:

### App Registration

1. Create a new App Registration
1. Create a Client Secret (used for backend)
1. Add the delegated permissions `Mail.Read` and `Mail.Send` under Graph
1. Expose the Api using the default url
1. Create a new scope `access_as_user` for users and admins
1. Add the access tokens `email` and `upn` (with permissions)
1. Under `Authentication` add the endpoint of your static website (after creating it)

### Single Page Application

1. Create a `Storage Account` in Azure
1. Add a static website using the `index.html` as entrypoint
1. Substitute the settings of `.env-cmdrc` with your own
1. Build the web app in `.\spa` using `npm run build`
1. Upload the contents of `.\build` to the `$web` container of the `Storage Account`

### Function App

1. Create a `Function App` in Azure (for .NET 7)
1. Publish the app to the function app (possibly with Visual Studio)
1. Add CORS rule for the endpoint of your static website
1. In the App Settings add settings for Graph and OpenAi connections (e.g. `Graph:TenantId`)
