# Sample to reproduce issue with Azure Front Door Service and ASP .NET Core

This is a basic setup, the app is a new mvc app and need deployed to an Azure App Service. You can execute the *setup.sh* file.

I have placed git tags with the different iterations and will outline the results for each tag in this README.

I have also wired it up to display the host name on the home page.

```Bash
$ ./setup.sh
```

Alternatively, you can just run the following Azure CLI command to create the App Service and deploy the app.

```Bash
$ az webapp up --name fds-issue --sku B1
```

Once that is created, you will need to log into the portal and do some configuration to enable logging. 

## Enable Logging in the App Service

Now to get the logs, we need to do the following:

1. Navigate to the App Service that was created.
2. Go to the Monitoring section in the side panel.
3. Click on App Service Logs
4. Turn on the Application Logging, set the log level to *Information*, and then save.

Now we can send a request and see what headers are included before we put it behind a Front Door Service.

It looks we have the following in the logs

```
X-Forwarded-For:  <Should be your IP>
X-Forwarded-Proto: https
```

Complete results from the logs

## Adding the Front Door Service

Create a Front Door service and add the app service as a backend, use the default rule that it wants to create.

According to the docs [here](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-http-headers-protocol) we should see the following headers:

* X-Forward-For
* X-Forwarded-Proto
* X-Forwarded-Host

After adding the Front Door Service, here are the forewarded headers you should see the following:

```
X-Forward-For: <Not your IP, probably internal FDS IP, its not the external>
X-Forward-Proto: https
```

Looks like the *X-Forward-Host* header is missing from the request.

## What is happening so far?

We are not seeing our IP address coming from Front Door, nor are we seeing the X-Forward-Host header like the docs say should be there.

## Step 1: Clearing KnownNetworks and KnownProxies

I added the following to see this is causing the issue. The default behavior outlineded [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-2.2#forwarded-headers)
say these are set to loopback.

This is bad security, but I wanted to see if it will work.

It does work, see the *X-Original-* headers are populated and the host name on the app is reporting as the Front Door.

However, the values are not what are expected.

```
X-Original-For: <Internal Front Door IP, not my client IP>
```


## Step 2: Increasing Forward Limit

Hmm, I am going to try to increase the Forward Limit to see if this fixes the Client IP being report.

This change didn't have any affect on the IP.


## Step 3: Adding Known Hosts

Going to add Known Hosts to increase security. Just grab the FQDN of the Front Door and put in the Startup.cs file where the options are initialized.

Everything still works.


