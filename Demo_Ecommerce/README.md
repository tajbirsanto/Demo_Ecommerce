# Demo E-commerce with ManyDial Integration

A demo e-commerce website showcasing ManyDial's automated call services integration.

## Features

- 🛒 **E-commerce Store** - Product catalog, shopping cart, checkout
- 📞 **Call Automation** - Automated order confirmation calls via ManyDial
- 📱 **Call Center** - Embedded call center interface for customer support
- 📊 **Webhook Logs** - Real-time logging of ManyDial events
- 💾 **SQLite Database** - Persistent storage for products and orders

## Tech Stack

- ASP.NET Core 9.0
- Entity Framework Core with SQLite
- Bootstrap 5 + Vanilla JavaScript
- ManyDial API Integration

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio Code or Visual Studio

### Run Locally

```bash
cd Demo_Ecommerce
dotnet restore
dotnet run
```

Open http://localhost:5009 in your browser.

### Configuration

Update `appsettings.json` with your ManyDial API key:

```json
{
  "ManyDial": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "CallerId": "+8809638013032"
  }
}
```

## ManyDial Integration

This demo showcases:

1. **Caller ID Request API** - Register business caller IDs
2. **Call Automation API** - Dispatch automated calls with IVR menus
3. **Call Center API** - Create and manage call centers
4. **Agent Management** - Add/remove agents
5. **Click-to-Call** - Initiate calls from web interface
6. **Webhooks** - Receive call delivery and status updates

## Deployment

### Deploy to Render (Free)

1. Push code to GitHub
2. Go to [render.com](https://render.com)
3. Create new Web Service
4. Connect your GitHub repo
5. Use Docker deployment
6. Set environment variables for API keys

## License

MIT License - Built for ManyDial Integration Demo
