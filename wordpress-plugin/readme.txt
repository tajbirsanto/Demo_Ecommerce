=== ManyDial WooCommerce Integration ===
Contributors: manydial
Tags: woocommerce, calls, order confirmation, IVR, ManyDial
Requires at least: 5.0
Tested up to: 6.4
Stable tag: 1.0.0
Requires PHP: 7.4
License: GPLv2 or later
License URI: https://www.gnu.org/licenses/gpl-2.0.html

Integrate ManyDial automated call confirmation with your WooCommerce orders.

== Description ==

**ManyDial WooCommerce Integration** connects your WooCommerce store with ManyDial's automated call system. When a customer places an order, you can dispatch an IVR call that:

* **Press 1** — Customer confirms the order → Order status auto-updates to "Processing"
* **Press 2** — Customer wants to talk → Call forwards to your number
* **Press 3** — Customer cancels → Order status auto-updates to "Cancelled"

### Features

* 📞 **One-Click Call** — Call customers directly from the order edit page
* 📋 **Bulk Call** — Select multiple orders and call all customers at once
* 🔄 **Auto-Call** — Optionally auto-call customers when new orders arrive
* 🌐 **Bangla & English** — Full support for Bangla (bn-BD) and English voice messages
* 📊 **Call Tracking** — See call status, duration, and customer response on each order
* 🔗 **Webhook** — Automatic order status updates based on customer's keypress
* 🎯 **Call Forwarding** — Forward calls to your number when customer presses 2

### How It Works

1. Install and activate the plugin
2. Go to **WooCommerce → ManyDial** and enter your API key and Caller ID
3. On any order, click **"ManyDial: Call Customer"** from the order actions dropdown
4. The customer receives an automated call with order details
5. Based on their keypress (1/2/3), the order status updates automatically

== Installation ==

1. Upload the `manydial-woocommerce` folder to `/wp-content/plugins/`
2. Activate the plugin through the **Plugins** menu in WordPress
3. Go to **WooCommerce → ManyDial** to configure your API credentials

### Configuration

| Setting | Description |
|---------|-------------|
| API Key | Your ManyDial API key (from dashboard) |
| Caller ID | Your ManyDial caller ID number |
| Forward Number | Number to forward calls to (optional) |
| Language | Voice message language (Bangla/English) |
| Voice | Male or Female voice |
| Auto-Call | Enable/disable auto-call on new orders |

### Webhook Setup

Copy the webhook URL shown on the settings page and paste it in your ManyDial dashboard as the **Delivery Hook URL**.

== Frequently Asked Questions ==

= Where do I get my API key? =
Sign up at [manydial.com](https://manydial.com) and get your API key from the dashboard.

= Does it work with Bangla? =
Yes! The plugin supports Bangla (bn-BD) voice messages with natural conversational tone.

= Can I customize the voice message? =
The voice message automatically includes the customer name, order number, items, and total. Language and voice gender can be configured in settings.

= What happens if the customer doesn't answer? =
The webhook logs "no response" on the order. You can then retry the call manually.

== Changelog ==

= 1.0.0 =
* Initial release
* WooCommerce order action integration
* Bulk call support
* Auto-call on new order (optional)
* Webhook for automatic order status updates
* Bangla and English voice support
* Call status column in orders list
* ManyDial meta box on order detail page
* Quick call button on order edit page
* HPOS compatibility

== Upgrade Notice ==

= 1.0.0 =
First release of ManyDial WooCommerce Integration.
