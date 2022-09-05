# GameLauncher.cs

This GameLauncher is created with C#.
I started building a launcher myself because I couldn't find good ones on the internet.
This launcher can update your game client using .zip files. It basically downloads the zip and extract them in current working dir.

I decided to make this opensource for any other private server developers. I hope you like it!

![alt tag](http://puu.sh/qkWBe/1fb38b9c82.png)

Remote file needs this format.
```
[General]
force_Start=true
patch_url=http://example.com/patches/data/
news_url=http://google.nl/

[Client]
client_path=client/client.exe
client_parameters=127.0.0.1 1000

[Patches]
local_list=client/patchlist.dat
patchcount=1
patch1=Data.zip
```
