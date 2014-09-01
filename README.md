IIS-CI
======

IIS Continuous Integration from Source Control.

Features
--------
1. TFS/Git Integration (without TFS/Git installation on server, You still need to purchase TFS CAL from Microsoft)
2. Downloads only modified source
3. XDT Support for web.config at root folder


Current Status
--------------
IISCI.Build project successfully downloads TFS/Git Project and executes MSBuild script.

Upcoming Items
--------------

1. IIS Site Editor to configure build-config.json file
2. IIS Web Hook to automatically fetch-build-deploy
 
Why not use Kudu?
----------------

1. Project Kudu has dependency on Git and Node, both are typically not part of .NET development workflow. 
2. Besides Kudu creates kudu services website for each hosted website, IIS with 100s of sites are difficult to recreate and redeploy using kudu.
3. Kudu does not work with TFS.
4. IIS-CI needs to be installed only once per server, and config file can manage access rights.

