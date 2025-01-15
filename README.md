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


Pokyny k testování aplikace:

Otevření eventu přes Calendar:

Při spuštění aplikace se otevře calendar. Pokud jste na jiném panelu, rozklikněte tři vodorovné čárky vlevo nahoře, symbolizující “Menu” a zde zvolte “Calendar”.

Zde můžete projíždět nastávající dny. Pokud je v ten den naplánování nějaký event, zobrazí se hned pod číslem.

Pro zjištění bližších informací o eventu, rozklikněte 16. ledna 2025. Měla by se zobrazit hláška “No events for this date”, což značí že žádný event není na tento den přiřařený.

Pro vrácení na kalendář poklepejte na šipku vlevo nahoře, do té doby než se ocitnete zpátky na okně “Calendar”.

Rozklikněte políčko 19. ledna 2025, dále zvolte první event s nápisem “Test”.

Přejděte z ledna 2025 na únor 2025 pomocí šipek vpravo od názvu měsíce. Poté rozklikněte políčko 5. února pro zobrazení eventu.

Otevření eventu přes Event List:

Pro zobrazení Event list jděte do levého horního rohu a klikněte na tři vodorovné čárky symbolizující “Menu” a vyberte “Event List”.

Zde se vám zobrazí všechny eventy, které byly do skupiny přidané, a jejich datum přidání. 

Pro zobrazení bližších informací o eventu, lze rozkliknout libovolný event pro bližší informace k eventu.

Najděte a rozklikněte event se jmenem “ukol”, který se budou dít 19. ledna 2025.
