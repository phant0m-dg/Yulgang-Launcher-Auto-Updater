# GameLauncher.cs

This GameLauncher is created with C#.
I started building a launcher myself because I couldn't find good ones on the internet.
This launcher can update your game client using .zip files. It basically downloads the zip and extract them in current working dir.

I decided to make this opensource for any other private server developers. I hope you like it!

-Zolero
-Phant0m

![alt tag](http://puu.sh/qkWBe/1fb38b9c82.png)

Remote file needs this format.
```
[General]
patch_url=http://sarasa.com.ar/
news_url=http://sarasa.com.ar/

[Client]
client_path=client/client.exe
client_parameters=127.0.0.1 1300

[Patches]
local_list=client/patchlist.dat
patchcount=5
patch1=Data1.zip
patch2=Data2.zip
patch3=Data3.zip
patch4=Data4.zip
patch5=Data5.zip

[LaucherVersion]
launcher_ver=1.0.2.3
patch=Launcher.zip
```

Change version in AssemblyInfo.cs to update the Launcher.