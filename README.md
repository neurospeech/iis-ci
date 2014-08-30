IIS-CI
======

IIS Continuous Integration from Source Control.

Current Status
--------------
IISCI.Build project successfully downloads TFS Project and executes MSBuild script.

Upcoming Items
--------------

1. IIS Site Editor to configure build-config.json file
2. IIS Web Hook to automatically fetch-build-deploy
3. Deploy using Source zip obtained from Git Sites like BitBucket, GitHub etc
 
Why not use Kudu?
----------------

1. Project Kudu has dependency on Git and Node, both are typically not part of .NET development workflow. 
2. Besides Kudu creates kudu services website for each hosted website, IIS with 100s of sites are difficult to recreate and redeploy using kudu.
3. Kudu does not work with TFS.
4. IIS-CI needs to be installed only once per server, and config file can manage access rights.

