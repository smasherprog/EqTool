# EqTool

<h2>THIS PROGRAM WORKS SOLELY BY READING YOUR LOG FILE. This is legal according to the <a href="https://www.project1999.com/forums/showthread.php?t=325349">rules.</a></h2>
<img width="851" alt="image" src="https://user-images.githubusercontent.com/3393733/214930917-994fa61b-f1c8-414b-9761-1c93ca247b63.png">

Instructions:
<ul>
<li>
<h2>Download the latest <a href="https://github.com/smasherprog/EqTool/releases/download/1.0.1.911/EQTool_1.0.1.911.zip">EQTool.zip</a>, Unzip it and run EQTool.exe</h2>
</li>
<li>The program runs in the system tray. Look there to reopen spells window or settings! Program will check for updates on startup and self update if required, but if you want to check for a new Update, use the menu in the system tray!</li>
</ul>
<h5>Why the pig?</h5>
<p>https://discord.gg/nSrz8hAwxM</p>
Features:
<br/>
<ul>
<li>Detect EQ directory location instead of user required to enter it.</li> 
<li>Detect Spells cast on others (this is a best guess as I am reading the log file so chloroplast and Regrowth of the growth have the same message)</li>
<li>Filter spells show by class</li> 
<li>Remove Spells from List if "Worn off message occurs"</li> 
<li>Mob Info Window gives details about mobs tht you con in game.</li>
<li>Automatically remove dead npc/player from the spell list.</li> 
<li>Auto detect level and class!</li>
<li>DPS is trailing 12 second average.</li>
<li>Maps</li>
<li>Timers (Only Minutes are supported)</li>
</ul>
<h5>Timers (Only Minutes are supported) -- All below commands work in regular say!</h5>
<ul>
<li>Timer Start Crypt Camp 35</li>
<li>Start Timer Crypt Camp 35</li>
<li>Timer Cancel Crypt Camp</li>
<li>Cancel Timer Crypt Camp</li>
</ul>
<img width="1150" alt="image" src="https://user-images.githubusercontent.com/3393733/222487918-bf41211b-439e-4d2e-8b4d-e2a5f09a2bbd.png">

<h4>System Tray Icon</h4>
<img width="152" alt="image" src="https://user-images.githubusercontent.com/3393733/212717141-6e26b9af-660a-493d-9f73-2c3464b7c224.png">

<h4>TO DO List</h4>
<ul>
<li>Prevent Map panning outside of bounds</li> 
<li>Release to single exe, not a zip</li> 
<li>Fix overlapping Map on window</li>
<li>Determine pet level based on the maxhit and add to the DPS window</li>
<li>Add location to mouse hover on map</li>
<li>Seive Counter</li> 
<li>Enable EQ logging automatically if EQ is not running.</li>
<li>Add option to auto prune eq log file. EQ logfiles can cause issues with EQ itself if they get too large!</li>
<li>Self update when NOT in use</li>
<li>Raid Group suggestions for guild: AOE; CH Chain; AOE+Ch Chain; Other</li>
<li>Better track players levels and classes</li>
<li>Respawn Time in Mob Info window</li>
<li>Ability Hide/show mob info data</li>
<li>Automatically add timer when named npc dies. Use Wiki for notable npc names</li>
<li>Add donals BP to timers list</li> 
<li>Rename Application to Pig Parse</li>
<li>Enrage alert/advanced alert.</li>
<li>charm break alert</li>
<li>charm spell effect removal</li> 
<li>Map window add toggle to follow location</li>   
<li>Add loot TAB to Mob Window. This tab will show item name, looted from, player name who looted, and unix geek price data, running total looted.</li>
</ul>

<h3>Faqs</h3>
<h4>Why does chrome warn me?</h4> 
<img width="359" alt="image" src="https://user-images.githubusercontent.com/3393733/223326270-a079946d-57dc-41dd-a58e-f46a0c776b54.png">
<ul>
<li>Ignore it and download!</li> 
</ul>
<h4>Why does windows pop up a blue screen?</h4> 
<img width="427" alt="image" src="https://user-images.githubusercontent.com/3393733/223328194-0946d278-09dc-4504-bed8-172d63fa98e0.png">
<ul>
<li>Press More info and "Run anyway"</li> 
</ul>
<h4>Why does the program not start?</h4> 
<img width="944" alt="image" src="https://user-images.githubusercontent.com/3393733/223326377-7cab3be5-bee5-4029-b513-0e8b2ff0bb78.png">
<ul>
<li>In the above image, you are running the exe from INSIDE the zip. You must first EXTRACT the exe, then you can run it!</li> 
</ul>
<h4>Why does my settings window say Configuration missing?</h4> 
<img alt="image" src="https://user-images.githubusercontent.com/3393733/222051822-fc4b750d-2efa-4eb9-bc00-589d3cc5b781.png">
<ul>
<li>EQTool was unable to automatically detect your P99 install folder. You must specific it yourself!</li>
<li>EQTool detected that eq logging is turned off. You must click enable logging. This will turn on EQ's logging which is where EQTool gets informatioon from.</li>
</ul>

<h4>Do i have to set my class and level?</h4> 
<ul>
<li>If you cast spells eqtool will automatically detect your class and level once you start casting spells.</li>
<li>You should still enter your class and level. It helps ensure calculations on spell durations are accurate.</li>
</ul>

<h4>I only care about spells cast on me, not everyone else!</h4> 
<ul>
<li>Great, goto settings and make sure the box is checked; 'Only show spells that effect you'.</li> 
</ul>

<h4>I only want to see cleric buffs; there are too many buffs to see!</h4> 
<ul>
<li>Great, goto settings and make sure that cleric is the only class selected in the "Other Spells" section.</li> 
</ul>

<h4>I have everything working, but i dont see my location on the map, why?</h4> 
<ul>
<li>You need to type /loc into chat so that your location is feed to the log file.</li> 
<li>Normally, players create a hotkey that is bound to their movement keys. Then add a /loc so that each time you move, the macro for /loc is called.</li> 
<li>I set up my movement keys 'a' and 'd' to activate my hotbar 1 macro which has a /loc in it.</li> 
</ul>

<h4>How do i get the latest update?</h4> 
<ul>
<li>Goto the system tray, click the pig icon and goto check for updates.</li> 
<li>Updates are checked for every timee the application starts as well.</li> 
<li>If an update is available it will download and start the new version. The old version will be deleted.</li> 
</ul>

<h4>Why do the spell effect still show for dead npcs?</h4> 
<ul>
<li>This program reads your log file. So, if you were too far away to see the 'slain' message, then there is no way for EQTool to know the npc is dead.</li> 
</ul>

<h4>Why do the see spell effects for others even though I checked the box 'Only show spells that effect you?'</h4> 
<ul>
<li>EQTool will ALWAYS show detrimental spell effects on everyone. The reason is that EQTool CANNOT tell the difference between an NPC and a PC. This means that in order to show spell effects on NPC, like slow/tash, etc, i must show ALL detrimental spell effects on all NPS/PC alike.</li> 
</ul>

<h4>How do I remove an NPC/Player from the spell list?</h4> 
<img width="218" alt="image" src="https://user-images.githubusercontent.com/3393733/222474771-41cc3276-9a9e-4a30-b868-5fb4d0b87de4.png">
<ul>
<li>This can happen for many reasons. The most common being that they left the group and you want to remove them from the list.</li>
<li>Click the Trashcan next to the next and that NPC/Player will be removed from the list. This is a one-time action and if you cast on them again, they will reappear.</li> 
</ul>
