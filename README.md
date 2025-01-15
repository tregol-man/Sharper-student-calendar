Petr Hamera,
Ori Rowan,
Tadeáš Antonín Pliska,
Filip Soukup,
Jan Brunclík
--------------------------
Sharper student slouží k vytvoření skupiny pro sdílení informací a materiálu k vzdělávání. Jeho hlavní myšlenkou je sjednotit informace na aplikacích, které jsou využívání studenti, jako například Google Keep, Google disk a Discord.
Aplikace má kalendář, do kterého můžou uživatelé ve skupině přidávat události, které jsou zobrazeny ostatním studentům. Tyto události mohou obsahovat i soubory pro např. obrázky.
------------------------

K provozu je potřeba mít stažení Newtonsoft.json

Návod na build aplikace:
Stačí napsat následující příkaz do konzole a aplikace se zhotoví

dotnet publish -c Release -r android-arm64 -p:PackageFormat=Apk -f net8.0-android --sc true

Build se poté uloží zde:
Sharper-student-calendar-experimental\Calendar\bin\Release\net8.0-android\android-arm64
