# BotConsole

## Overview
BotConsole is a console application designed to demonstrate the capabilities of a simple ant+ powermeter bot. It is built using .NET Framework 4.8.1.

## Features
- Basic ant+ powermeter/cadence bot
- Random noise so that it doesn't look too obviously like a bot
- Joule/watt limits so that it doesn't exceed 20m power thresholds
- Simple drafting logic
- TT mode (where draft is ignored)
 
## Requirements
- .NET Framework 4.0 or .NET Framework 4.8.1
- Visual Studio

## Getting Started
1. Clone the repository.
2. Open the solution in Visual Studio.
3. Build the solution to restore the necessary packages.
4. Extract the ant+ libraries from thisisant.com to the correct location.  I didn't include them in this project because it's not my code to share.
1. Copy the wrapper dll to the correct location.
1. Set the configuration values in the app.config file of the BotConsole project.
1. Set the configuration values on the appsettings.json file of the ZwiftDataCollectionAgent.
5. Run both applications.

## Usage
- There are two console applications.  ZwiftDataCollectionAgent.Console collects data from zwift and puts them in an sqlite database.   BotConsole.Console is the bot that reads the data from the database and sends it to the ant+ powermeter.  In the literal sense, the zwiftDataCollectionAgent isn't absolutely necessary, but provides the bot with information in game.

## Contributing
1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -am 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Create a new Pull Request.

## Future Enhancements
* Hill detction
* Joule/watt limits so that it doesn't exceed 1m 5m and 10m power thresholds
* Simulated HRM
* Multiple rider support
