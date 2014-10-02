IIS-CI
======
IIS Continuous Integration/Deployment from Git & TFS Source Control.

Features
--------
1. TFS/Git Integration (without TFS/Git installation on server, You still need to purchase TFS CAL from Microsoft)
2. Downloads only modified source
3. XDT Support for web.config at root folder
4. Web Hook to automatically fetch-build-deploy


Current Status
--------------
Ready to use as basic deployment controller.


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
2. Enable Anonymous Authentication for Triggers to work.
3. Set LocalSystem as Application Pool Identity.
4. Copy "C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\*" files on the server
5. Open the website, you will see list of IIS sites with config and build options.
6. You can configure the source type as Git or TFS, Git should use https transport only.
7. Enter username/passwords for remote Git
8. Specify Solution Path as relative path of .sln file within the git source code without front slash.
9. Specify Web Project Path as relative path of .sln file within the git source code without front slash.
10. Add AppSettings you would want to overwrite after deployment.
11. Add ConnectionStrings you would want to overwrite after deployment.
12. After config is saved correctly, click on Build link.
