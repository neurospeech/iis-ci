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
Ready to use as basic deployment controller.

Pending Items
--------------

1. IIS Web Hook to automatically fetch-build-deploy
 
Why not use Kudu?
----------------

1. Project Kudu has dependency on Git and Node, both are typically not part of .NET development workflow. 
2. Besides Kudu creates kudu services website for each hosted website, IIS with 100s of sites are difficult to recreate and redeploy using kudu.
3. Kudu does not work with TFS.
4. IIS-CI needs to be installed only once per server, and config file can manage access rights.

Building
--------

You can use Visual Studio 2012/2013 to build from source code and deploy generated files in IIS.

Installation
------------

1. Create a website in IIS, make sure Windows Authentication is enabled with NTLM being first provider.
2. Set LocalSystem as Application Pool Identity.
3. Copy "C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\*" files on the server
4. Open the website, you will see list of IIS sites with config and build options.
5. You can configure the source type as Git or TFS, Git should use https transport only.
6. Enter username/passwords for remote Git
7. Specify Solution Path as relative path of .sln file within the git source code without front slash.
8. Specify Web Project Path as relative path of .sln file within the git source code without front slash.
9. Add AppSettings you would want to overwrite after deployment.
10. Add ConnectionStrings you would want to overwrite after deployment.
11. After config is saved correctly, click on Build link.

