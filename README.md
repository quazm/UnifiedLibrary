AI generated. Vs 2022.
Тестировалось в jellyfin 10.11.11 + Plugin Pages 2.4.11.0 + File Transformation 2.5.11.0 (нужно поставить оба плагина предварительно добавив репозиторий https://www.iamparadox.dev/jellyfin/plugins/manifest.json)

В конфиге плагина (<Server_Data>\plugins\configurations\UnifiedLibrary.xml) добавить guid библиотек (можно выдернуть из урл при открытии конкретной библиотеки), которые надо выводить как одну:
```xml
<?xml version="1.0" encoding="utf-8"?>
<PluginConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Enabled>true</Enabled>
  <IncludedLibraryIds>
    <string>f137a2dd21bbc1b99aa5c0f6bf02a805</string>
    <string>4c758bc444f5dbf2a974b56ace5618aa</string>
	<string>f8019b455f0398d647f592d2c52cf6f2</string>
	<string>b16a6d3d991193b943cd6e27bb0c6650</string>
	<string>1788765f5851298a771ce0a13c045920</string>
  </IncludedLibraryIds>
  <IncludedItemTypes>
    <BaseItemKind>Movie</BaseItemKind>
  </IncludedItemTypes>
  <DefaultPageSize>100</DefaultPageSize>
</PluginConfiguration>
```
В конфиг Plugin Pages (<Server_Data>\plugins\configurations\Jellyfin.Plugin.PluginPages\config.json):
```json
{
  "pages": [
    {
      "Id": "unified-library",
      "DisplayText": "Все фильмы",
      "Url": "/Plugin/UnifiedLibrary/Page",
      "Icon": "movie"
    }
  ]
}
```
В веб-конфиг jellyfin (<Server>\jellyfin-web\config.json - это не каталог с датой!) добавить menuLinks, что бы ссылка в панели слева, сверху:
```json
  "menuLinks": [
    {
      "name": "Все фильмы",
      "icon": "movie",
      "url": "#/userpluginsettings.html?pageUrl=/Plugin/UnifiedLibrary/Page"
    }
  ],
```

Выглядит как говно, т.е. как-то вот так:

<img width="1551" height="871" alt="{62C46B28-9157-41DE-BD53-676B0668EE45}" src="https://github.com/user-attachments/assets/3c542afe-83c4-460b-b532-3d645efd9e33" />

