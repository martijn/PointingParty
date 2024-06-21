![Pointing Party logo](PointingParty/wwwroot/pointpingparty-cactus-light.svg)

# Pointing Party

Pointing Party is a web application to aid in agile story point estimation processes with minimal barriers
to entry. Simply start a game, share the URL, and start voting. Registration is neither necessary nor possible
and the application is built to be usable from all platforms and devices.

The project does not rely on a backend database. Instead, each client maintains its own game state
based on events published through the central SignalR hub.

This project uses .NET 9, Blazor, and Tailwind CSS.

## Try it out

The application can be found at https://pointingparty.com/. It comes without warranty, but feel
free to use it in your team.

## Contributing

Simply clone and run. Pull requests are welcome!

The project contains some unit tests, which can be executed with `dotnet test` or from the IDE. In addition,
a Playwright test suite can be found in the `e2e/` directory. To run the end-to-end tests in development,
start the application and run the following command in the `e2e/` directory:

```
npm install
BASE_URL=http://localhost:5174 npx playwright test --ui
```

© Martijn Storck
