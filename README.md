# PigParse
<img width="1892" alt="image" src="https://github.com/smasherprog/EqTool/blob/main/Example.png?raw=true">

<h2>THIS PROGRAM WORKS SOLELY BY READING YOUR LOG FILE.</h2>
 
Instructions:
<ul>
<li>
<h2>Project 1999: Download <a href="https://github.com/smasherprog/EqTool/releases/download/3.24.115.2/EQTool_Project19993.24.115.2.zip">EQTool_Project1999.zip</a>, Unzip it and run EQTool.exe</h2>
</li>
<li>
<h2>Project Quarm: Download <a href="https://github.com/smasherprog/EqTool/releases/download/4.24.115.2/EQTool_Quarm4.24.115.2.zip">EQTool_Quarm.zip</a>, Unzip it and run PQTool.exe</h2>
</li>
<li>The program runs in the system tray. Look there to reopen spells window or settings! Program will check for updates on startup and self update if required, but if you want to check for a new Update, use the menu in the system tray!</li>
</ul>
Features:
<br/>
<ul>
<li>Kael faction pull timer Automatically added for everyone regardless of what zone you are in. </li> 
<li>Automatically add timers on death of npc so you dont have to!</li> 
<li>Detect EQ directory location instead of user required to enter it.</li> 
<li>Detect Spells cast on others (this is a best guess as I am reading the log file so chloroplast and Regrowth of the growth have the same message)</li>
<li>Filter spells show by class</li> 
<li>Remove Spells from List if "Worn off message occurs"</li> 
<li>Mob Info Window gives details about mobs that you con in game.</li>
<li>Automatically remove dead npc/player from the spell list.</li> 
<li>Auto detect level and class!</li>
<li>DPS is trailing 12 second average.</li>
<li>Show fight Session data for comparisons.</li>
<li>The following spells have counters that are automatically tracked:Mana Seive, LowerElement (Flux Staff Effect), Concussion, Flame Lick, Cinder Jolt, Jolt.</li> 
<li>The following Overlays are available: Enrage; Levitate fading; invis fading; Charm Break; and Ch Chain</li>
<li>The following Audio alerts are available: Enrage; Levitate fading; Charm Break; and invis fading</li>
<li>Timers on map for easy TOD tracking!</li>
<li>All Melee disciplines show in the cooldown section of the timers window.</li>
<li>Pricing information in Mob info window.</li>
<li>Save spells cast on you when you log out so when you log back in they resume counter!</li>
<li>Maps</li>
<li>Automatic self update.</li>
<li>Timers (Only Minutes are supported)</li>
</ul>
<h5>Ch format is loosely: TAG POSITION CH TARGET</h5>
<ul> 
 <li>
  TAG is optional and can be used to filter out other chains that are going on. For example, if you TAG is CA, it will only show chains that have that TAG in it.
 </li>
 <li>
  POSITION must be 3 in length, and follow a format of: 001-999; or AAA-ZZZ
 </li>
 <li>The characters " ch " must be present in the line, capitalization doesnt matter.</li>
 <li>All instances of TAG are stripped out of text. The characters " ch " are stripped out. The first group of 3 characters that are either all the same letter or a number are assigned as the position. </li>
 <li>All instances of Position are stripped out of the text. All non alphanumeric are stripped out of the text. What remains is the target.</li>
</ul>
<p>Tag MUST BE 3 in length! The following are included in tests that are supported, This is not all the possible ways, but to show examples of what i have support for!</p></p>

<ul>
<li>Curaja shouts, 'GG 014 CH -- Wreckognize'</li>
<li>Hanbox shouts, 'GG 001 CH -- Beefwich'</li>
<li>Hanbox shouts, 'GG 001 CH --Beefwich'</li>
<li>Wartburg says out of character, 'CA 004 CH -- Sam'</li>
 <li>Wartburg says out of character, '004 CH - Sam'</li>
  <li>Hanbox tells the guild, 'GG 001 CH --Beefwich'</li>
  <li>You say out of character, 'CA 002 CH -- Aaryk'</li>
  <li>Windarie auctions, '111 --- CH << Mandair  >> --- 111'</li>
  <li>Mutao auctions, '777 CH <>> Mandair <<> 777'</li>
  <li>Mutao auctions, 'AAA CH <>> Mandair <<> AAA'</li>
  <li>Mutao auctions, 'GGG AAA CH <>> Mandair <<> AAA'</li>
  <li>Mutao auctions, 'BBB CH <>> Mandair <<> BB'</li>
  <li>Mutao auctions, 'AAA CH <>> Mandair <<>'</li>
</ul>
<h5>Timers (Only Minutes are supported) -- All below commands work in regular say!</h5>
<ul>
<li>Timer Start Crypt Camp 35</li>
<li>Start Timer Crypt Camp 35</li>
<li>Timer Cancel Crypt Camp</li>
<li>Cancel Timer Crypt Camp</li>
</ul>
<img width="1624" alt="image" src="https://github.com/smasherprog/EqTool/assets/3393733/3c53a1d8-44c4-499b-9e92-ea5d5f38275e">
<h4>CH Chain overlay Below</h4>
 <img width="1174" alt="update1" src="https://github.com/smasherprog/EqTool/assets/3393733/86c08360-48d9-42c4-9a86-7fea652d8133">
 <h4>FTE Overlay (includes Guild)</h4>
<img width="1548" alt="image" src="https://github.com/smasherprog/EqTool/assets/3393733/92446f74-3f7b-4957-a712-43fe9d6a3191">

<h4>System Tray Icon</h4>
<img width="152" alt="image" src="https://user-images.githubusercontent.com/3393733/212717141-6e26b9af-660a-493d-9f73-2c3464b7c224.png">

<h4>TO DO List EQTool</h4>
<ul>  
   <li>Sharing of triggers for those nearby. </li>  
  <li>Add ring war triggers. </li>  
  <li>Map center on you.</li>  
   <li>Add Window Snapping code so windows can be grouped and all moved together.</li>   
 <li>Remove timers from map when timers removed from list and visa versa.</li>   
 <li>Add code to show MOB hits per second.</li>   
 <li>Port application to be cross platform(mac, win, linux).</li>   
 <li>Add Ability to bind a hotkey to toggle a window open/close.</li>    
 <li>Add group suggestions for each raid mob in game.</li>     
 <li>Make timers resetable.</li>   
 <li>Fix buff timers when updating, when update occurs buffs dissapear.</li> 
 <li>Add zone dump of names.</li> 
 <li>Add context menu to toggle pather vs static.</li>  
 <li>Add code to detect pet and always show buffs for your pet.</li> 
<li>Tab under Mob info window for last few minutes of Random rolls ordered by highest to lowest. </li>   
<li>Add visual cue that buff is under 1 minute left. </li> 
<li>Add option to enable OTHERS hits for DPS meter. </li>  
<li>Add Waypoint Command to add loc to map</li>    
<li>Map window add toggle to follow location</li>     
<li>Determine pet level based on the maxhit and add to the DPS window</li> 
<li>Add option to auto prune eq log file. EQ logfiles can cause issues with EQ itself if they get too large!</li>
</ul>

<h4>TO DO List PigParse Pricing</h4>
<ul>
<li>N/A </li> 
</ul>

<h3>FAQS</h3>
<h4>Why does chrome warn me?</h4> 
<img alt="image" src="https://user-images.githubusercontent.com/3393733/223326270-a079946d-57dc-41dd-a58e-f46a0c776b54.png">
<ul>
<li>Ignore it and download!</li> 
</ul>
<h4>Why does windows pop up a blue screen?</h4> 
<img height="300" alt="image" src="https://user-images.githubusercontent.com/3393733/223328194-0946d278-09dc-4504-bed8-172d63fa98e0.png">
<ul>
<li>Press More info and "Run anyway"</li> 
</ul>
<h4>Why does the program not start?</h4> 
<img height="300" alt="image" src="https://user-images.githubusercontent.com/3393733/223326377-7cab3be5-bee5-4029-b513-0e8b2ff0bb78.png">
<ul>
<li>In the above image, you are running the exe from INSIDE the zip. You must first EXTRACT the exe, then you can run it!</li> 
</ul>
<h4>Why does my settings window say Configuration missing?</h4> 
<img alt="image" src="https://user-images.githubusercontent.com/3393733/222051822-fc4b750d-2efa-4eb9-bc00-589d3cc5b781.png">
<ul>
<li>EQTool was unable to automatically detect your P99 install folder. You must specify it yourself!</li>
<li>EQTool detected that eq logging is turned off. You must click enable logging. This will turn on EQ's logging which is where EQTool gets information from.</li>
</ul>

<h4>Do I have to set my class and level?</h4> 
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
<img alt="image" src="https://user-images.githubusercontent.com/3393733/222474771-41cc3276-9a9e-4a30-b868-5fb4d0b87de4.png">
<ul>
<li>This can happen for many reasons. The most common being that they left the group and you want to remove them from the list.</li>
<li>Click the Trashcan next to the next and that NPC/Player will be removed from the list. This is a one-time action and if you cast on them again, they will reappear.</li> 
</ul>

<h4>Where and how are sieves tracked?</h4>  
<img alt="image" src="https://user-images.githubusercontent.com/3393733/230679391-92754265-ec66-4643-ad30-5b6e4cdd164b.png">
<ul>
<li>Every time a message for the enchanter Mana sieve spell is encountered, the counter is incremented.</li>
<li>So, if you are in range of the sieve messages, you will have an accurate count in the Trigger list.</li> 
</ul>

<h4>What is this DPS session tracking?</h4>  
<img alt="image" src="https://user-images.githubusercontent.com/3393733/230679632-732b2d60-e471-4508-bcfe-720f37ae2c35.png">
<ul>
<li>The first row is the saved fight data which shows all-time-best DPS, Biggest hit and Total Damage.</li>
<li>Current Session is best DPS, Biggest hit and Total Damage, except the timeframe starts since you have logged on.</li>
<li>Last Session is the old session data. You can use this to compare weapon swapping and differnt setups.</li>
</ul>

<h4>Why cant I see others DPS? I only see my own!</h4>  
<img alt="image" src="https://user-images.githubusercontent.com/3393733/232231377-1259ae63-f644-4e49-a246-8f1101f71190.png">
<ul>
<li>Turn on others hits in your eqsettings!</li>
</ul>

<h4>Map timers, how do they work?</h4>  
<ul>
<li>Right click in the map and a menu will appear, add the time, and press add.</li>
<li>You can move the timer around by dragging it.</li>
<li>You can delete the timer by right clicking over it and  clicking the delete option.</li>
<li>Timer color starts at Green and changes to Red the closer it gets to zero time remaining. Timers will last for 4 minutes after the time has expired then automatically remove themselves.</li>
</ul>
