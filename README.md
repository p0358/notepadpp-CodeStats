notepadpp-CodeStats
=====================

[![Github All Releases](https://img.shields.io/github/downloads/p0358/notepadpp-CodeStats/total.svg)](https://github.com/p0358/notepadpp-CodeStats/releases) [![GitHub release](https://img.shields.io/github/release/p0358/notepadpp-CodeStats.svg)](https://github.com/p0358/notepadpp-CodeStats/releases) [![Build status](https://ci.appveyor.com/api/projects/status/c34pe9fpnstehfe0?svg=true)](https://ci.appveyor.com/project/p0358/notepadpp-codestats)

Code::Stats - Write code, level up, show off! A free stats tracking service for programmers. This plugin enables XP tracking in Notepad++.


Installation
------------

1. Inside Notepad++ open the Plugins Admin (`Plugins` → `Plugins Admin...`).

2. Check the box next to `Code::Stats` in the list of plugins.

3. Click the `Install` button.

4. Confirm the restart of Notepad++.

3. Enter your [API token](https://codestats.net/my/machines) in the window that pops up, then press `OK`.

4. Use Notepad++ like you normally do and your amount of programming will be tracked for you automatically.

5. Visit https://codestats.net to see your logged stats.

OR

1. Go to [releases](https://github.com/p0358/notepadpp-CodeStats/releases) and download zip file for correct architecture of your Notepad++ installation.

2. Put it in Notepad++ plugin dir, inside of `CodeStats` folder, for example (for x86): `C:\Program Files (x86)\Notepad++\plugins\CodeStats`, or (for x64): `C:\Program Files\Notepad++\plugins\CodeStats`.

3. Start/restart Notepad++.

4. Enter your [API token](https://codestats.net/my/machines) in the window that pops up, then press `OK`.

5. Use Notepad++ like you normally do and your amount of programming will be tracked for you automatically.

6. Visit https://codestats.net to see your logged stats.


Anonymous usage statistics
---------------------

On the next Notepad++ launches, after API token has been provided, this plugin is making HTTPS request to analytics server containing plugin version and unique randomly generated ID. This is because author would like to see the amount of people using this. If you really do not want to be included, you can opt-out in plugin settings.


Troubleshooting
---------------

CodeStats for Notepad++ logs to `C:\Users\<user>\AppData\Roaming\Notepad++\plugins\config\CodeStats.log`.

Turn on debug mode (click the CodeStats icon in Notepad++) then check your log file.


Other info
----------

This plugin was originally based on [notepadpp-WakaTime](https://github.com/wakatime/notepadpp-wakatime)
