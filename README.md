# FlatScannerApp

## Description

I already found real estate for me and my wife so I am making this public so others can fork it, configure it and use it to help them found their own real estates. It's not 100% complete because this scrapper found perfect flat for us too quickly so I don't have any real motivation to develop it further... TODOs are below.

It's made in .NET 6, it uses background service which scrapps different real estate portals and extract data using XPath. Newly found real estate are then sent to the mail recipients. For now this is configured for Slovenian market but you can tailor it to your own desires. All exceptions while scrapping are also send to the dev team mails which is set in configuration. You can also deploy it as a Docker container.

Current portals (Providers):
 - Nepremicnine.net (https://www.nepremicnine.net/)
 - Bolha (https://www.bolha.com/)
 - Century 21 (https://c21.si/)
 - Nep24 (https://24nep.si/)
 - Galea (https://galea.si/)
 - Do Doma (https://www.dodoma.si/)

## Code

It's self-explanatory :)

Important files, folders, classes:
- FlatScannerService.cs (background service)
- Classes in Providers folder (eg. NepremicnineProvider.cs contains all logic to access nepremicnine.net portal and read data)
- Constants.cs (some configuration)
- appsetting.<env>.json:
  - configuration to enable/disable providers
  - configuration to use demo data (useful for development processes to get all XPath working correctly) or to use real data from web
  - configure gmail SMTP - check https://www.gmass.co/blog/gmail-smtp/ how to get app password for your account
  - configure mail recipients

## TODOs

Things that I still wanted to do but app already found perfect real estate:
- Move hardcoded urls with filters in query string from provider classes to configuration
- Move hardcoded filtering in cookies from provider classes to configuration
- Move intervals and timeouts from Constant class to configuration
- Create a bot prevention workaround (some portals have bot prevention to prevent access by automated processes like this one), for example Nepremicnine.net and Bolha providers sometimes return 302 for CAPTCHA solving url and maybe just 403 - Forbidden. My solutions for this were:
  - External proxy ZenRows https://www.zenrows.com/
  - Implement solution myself using solution from blog https://sangaline.com/post/advanced-web-scraping-tutorial/
- Change MailService to be more configurable, right now it always use gmail SMTP