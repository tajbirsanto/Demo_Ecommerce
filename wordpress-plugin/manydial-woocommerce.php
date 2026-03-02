<?php
/**
 * Plugin Name: ManyDial WooCommerce Integration
 * Plugin URI: https://manydial.com
 * Description: Integrate ManyDial automated call confirmation with WooCommerce orders. Adds a "Call Customer" button to orders and auto-confirms via IVR.
 * Version: 1.0.0
 * Author: ManyDial
 * Author URI: https://manydial.com
 * License: GPL v2 or later
 * License URI: https://www.gnu.org/licenses/gpl-2.0.html
 * Text Domain: manydial-woocommerce
 * Requires at least: 5.0
 * Requires PHP: 7.4
 * WC requires at least: 5.0
 * WC tested up to: 8.0
 */

if (!defined('ABSPATH')) {
    exit; // Exit if accessed directly
}

// Check if WooCommerce is active
if (!in_array('woocommerce/woocommerce.php', apply_filters('active_plugins', get_option('active_plugins')))) {
    add_action('admin_notices', function() {
        echo '<div class="error"><p><strong>ManyDial WooCommerce:</strong> WooCommerce must be installed and active.</p></div>';
    });
    return;
}

// ============================================================
// 1. SETTINGS PAGE
// ============================================================

add_action('admin_menu', 'manydial_add_settings_page');
function manydial_add_settings_page() {
    add_submenu_page(
        'woocommerce',
        'ManyDial Settings',
        'ManyDial',
        'manage_woocommerce',
        'manydial-settings',
        'manydial_settings_page_html'
    );
}

add_action('admin_init', 'manydial_register_settings');
function manydial_register_settings() {
    register_setting('manydial_settings', 'manydial_api_key');
    register_setting('manydial_settings', 'manydial_caller_id');
    register_setting('manydial_settings', 'manydial_forward_number');
    register_setting('manydial_settings', 'manydial_language', [
        'default' => 'bn-BD'
    ]);
    register_setting('manydial_settings', 'manydial_voice', [
        'default' => 'female'
    ]);
    register_setting('manydial_settings', 'manydial_auto_call', [
        'default' => 'no'
    ]);
}

function manydial_settings_page_html() {
    if (!current_user_can('manage_woocommerce')) {
        return;
    }
    ?>
    <div class="wrap">
        <h1>ManyDial Settings</h1>
        <form method="post" action="options.php">
            <?php settings_fields('manydial_settings'); ?>
            <table class="form-table">
                <tr>
                    <th scope="row"><label for="manydial_api_key">API Key</label></th>
                    <td>
                        <input type="password" id="manydial_api_key" name="manydial_api_key" 
                               value="<?php echo esc_attr(get_option('manydial_api_key')); ?>" 
                               class="regular-text" required>
                        <p class="description">Your ManyDial API key from the dashboard.</p>
                    </td>
                </tr>
                <tr>
                    <th scope="row"><label for="manydial_caller_id">Caller ID</label></th>
                    <td>
                        <input type="text" id="manydial_caller_id" name="manydial_caller_id" 
                               value="<?php echo esc_attr(get_option('manydial_caller_id')); ?>" 
                               class="regular-text" placeholder="+8801XXXXXXXXX" required>
                        <p class="description">Your ManyDial Caller ID number.</p>
                    </td>
                </tr>
                <tr>
                    <th scope="row"><label for="manydial_forward_number">Forward Number</label></th>
                    <td>
                        <input type="text" id="manydial_forward_number" name="manydial_forward_number" 
                               value="<?php echo esc_attr(get_option('manydial_forward_number')); ?>" 
                               class="regular-text" placeholder="+8801XXXXXXXXX">
                        <p class="description">Number to forward calls to when customer presses 2 (optional).</p>
                    </td>
                </tr>
                <tr>
                    <th scope="row"><label for="manydial_language">Language</label></th>
                    <td>
                        <select id="manydial_language" name="manydial_language">
                            <option value="bn-BD" <?php selected(get_option('manydial_language', 'bn-BD'), 'bn-BD'); ?>>বাংলা (Bangla)</option>
                            <option value="en-US" <?php selected(get_option('manydial_language', 'bn-BD'), 'en-US'); ?>>English (US)</option>
                            <option value="en-GB" <?php selected(get_option('manydial_language', 'bn-BD'), 'en-GB'); ?>>English (UK)</option>
                            <option value="hi-IN" <?php selected(get_option('manydial_language', 'bn-BD'), 'hi-IN'); ?>>हिन्दी (Hindi)</option>
                        </select>
                    </td>
                </tr>
                <tr>
                    <th scope="row"><label for="manydial_voice">Voice</label></th>
                    <td>
                        <select id="manydial_voice" name="manydial_voice">
                            <option value="female" <?php selected(get_option('manydial_voice', 'female'), 'female'); ?>>Female</option>
                            <option value="male" <?php selected(get_option('manydial_voice', 'female'), 'male'); ?>>Male</option>
                        </select>
                    </td>
                </tr>
                <tr>
                    <th scope="row"><label for="manydial_auto_call">Auto-Call on New Order</label></th>
                    <td>
                        <select id="manydial_auto_call" name="manydial_auto_call">
                            <option value="no" <?php selected(get_option('manydial_auto_call', 'no'), 'no'); ?>>No (Manual only)</option>
                            <option value="yes" <?php selected(get_option('manydial_auto_call', 'no'), 'yes'); ?>>Yes (Auto-call on new order)</option>
                        </select>
                        <p class="description">If enabled, ManyDial will automatically call customers when a new order is placed.</p>
                    </td>
                </tr>
            </table>
            <?php submit_button('Save ManyDial Settings'); ?>
        </form>

        <hr>
        <h2>Test Connection</h2>
        <p>
            <button type="button" class="button button-secondary" id="manydial-test-btn" onclick="manydialTestConnection()">
                Test API Connection
            </button>
            <span id="manydial-test-result" style="margin-left:10px;"></span>
        </p>

        <hr>
        <h2>Webhook URL</h2>
        <p>Set this in your ManyDial dashboard as the Delivery Hook URL:</p>
        <code><?php echo esc_url(rest_url('manydial/v1/webhook')); ?></code>

        <script>
        function manydialTestConnection() {
            var btn = document.getElementById('manydial-test-btn');
            var result = document.getElementById('manydial-test-result');
            btn.disabled = true;
            result.innerHTML = 'Testing...';
            
            fetch(ajaxurl + '?action=manydial_test_connection&_wpnonce=<?php echo wp_create_nonce("manydial_test"); ?>')
                .then(r => r.json())
                .then(data => {
                    result.innerHTML = data.success 
                        ? '<span style="color:green;">✓ Connected successfully!</span>'
                        : '<span style="color:red;">✗ ' + data.data + '</span>';
                    btn.disabled = false;
                })
                .catch(() => {
                    result.innerHTML = '<span style="color:red;">✗ Request failed</span>';
                    btn.disabled = false;
                });
        }
        </script>
    </div>
    <?php
}

// AJAX handler for test connection
add_action('wp_ajax_manydial_test_connection', 'manydial_test_connection_ajax');
function manydial_test_connection_ajax() {
    check_ajax_referer('manydial_test');
    
    $api_key = get_option('manydial_api_key');
    if (empty($api_key)) {
        wp_send_json_error('API key not configured');
        return;
    }

    $response = wp_remote_get('https://api.manydial.com/v1/portal/caller-id/list', [
        'headers' => [
            'x-api-key' => $api_key,
        ],
        'timeout' => 15,
    ]);

    if (is_wp_error($response)) {
        wp_send_json_error('Connection failed: ' . $response->get_error_message());
    } else {
        $code = wp_remote_retrieve_response_code($response);
        if ($code === 200) {
            wp_send_json_success('Connected');
        } else {
            wp_send_json_error('API returned status ' . $code);
        }
    }
    wp_die();
}

// ============================================================
// 2. ORDER ACTIONS — "Call Customer" BUTTON
// ============================================================

// Add "Call Customer" button to order actions dropdown
add_filter('woocommerce_order_actions', 'manydial_add_order_action');
function manydial_add_order_action($actions) {
    $actions['manydial_call_customer'] = '📞 ManyDial: Call Customer';
    return $actions;
}

// Handle the action
add_action('woocommerce_order_action_manydial_call_customer', 'manydial_process_call_action');
function manydial_process_call_action($order) {
    $result = manydial_dispatch_call($order);
    
    if ($result['success']) {
        $order->add_order_note('✅ ManyDial call dispatched to ' . $order->get_billing_phone());
    } else {
        $order->add_order_note('❌ ManyDial call failed: ' . $result['error']);
    }
}

// Add bulk action for calling multiple customers
add_filter('bulk_actions-edit-shop_order', 'manydial_bulk_actions');
function manydial_bulk_actions($bulk_actions) {
    $bulk_actions['manydial_bulk_call'] = '📞 ManyDial: Call Customers';
    return $bulk_actions;
}

add_filter('handle_bulk_actions-edit-shop_order', 'manydial_handle_bulk_call', 10, 3);
function manydial_handle_bulk_call($redirect_to, $action, $post_ids) {
    if ($action !== 'manydial_bulk_call') {
        return $redirect_to;
    }

    $success = 0;
    $failed = 0;

    foreach ($post_ids as $post_id) {
        $order = wc_get_order($post_id);
        if ($order) {
            $result = manydial_dispatch_call($order);
            if ($result['success']) {
                $success++;
                $order->add_order_note('✅ ManyDial call dispatched (bulk action)');
            } else {
                $failed++;
                $order->add_order_note('❌ ManyDial call failed: ' . $result['error']);
            }
        }
    }

    $redirect_to = add_query_arg([
        'manydial_called' => $success,
        'manydial_failed' => $failed,
    ], $redirect_to);

    return $redirect_to;
}

// Show bulk action result notice
add_action('admin_notices', 'manydial_bulk_action_notice');
function manydial_bulk_action_notice() {
    if (!empty($_REQUEST['manydial_called'])) {
        $called = intval($_REQUEST['manydial_called']);
        $failed = intval($_REQUEST['manydial_failed'] ?? 0);
        $msg = sprintf('ManyDial: %d call(s) dispatched successfully.', $called);
        if ($failed > 0) {
            $msg .= sprintf(' %d call(s) failed.', $failed);
        }
        echo '<div class="updated"><p>' . esc_html($msg) . '</p></div>';
    }
}

// ============================================================
// 3. AUTO-CALL ON NEW ORDER (optional)
// ============================================================

add_action('woocommerce_new_order', 'manydial_auto_call_on_order', 20, 2);
function manydial_auto_call_on_order($order_id, $order) {
    if (get_option('manydial_auto_call', 'no') !== 'yes') {
        return;
    }

    if (!$order) {
        $order = wc_get_order($order_id);
    }

    if (!$order || !$order->get_billing_phone()) {
        return;
    }

    // Small delay to ensure order is fully saved
    as_schedule_single_action(time() + 30, 'manydial_delayed_call', [$order_id], 'manydial');
}

// Fallback if Action Scheduler not available
add_action('manydial_delayed_call', 'manydial_execute_delayed_call');
function manydial_execute_delayed_call($order_id) {
    $order = wc_get_order($order_id);
    if (!$order) return;

    $result = manydial_dispatch_call($order);
    if ($result['success']) {
        $order->add_order_note('✅ ManyDial auto-call dispatched to ' . $order->get_billing_phone());
    } else {
        $order->add_order_note('❌ ManyDial auto-call failed: ' . $result['error']);
    }
}

// ============================================================
// 4. CORE CALL DISPATCH FUNCTION
// ============================================================

function manydial_dispatch_call($order) {
    $api_key = get_option('manydial_api_key');
    $caller_id = get_option('manydial_caller_id');
    $forward_number = get_option('manydial_forward_number');
    $language = get_option('manydial_language', 'bn-BD');
    $voice = get_option('manydial_voice', 'female');

    if (empty($api_key) || empty($caller_id)) {
        return ['success' => false, 'error' => 'ManyDial API key or Caller ID not configured'];
    }

    $phone = $order->get_billing_phone();
    if (empty($phone)) {
        return ['success' => false, 'error' => 'No phone number on order'];
    }

    // Clean phone number - ensure it has country code
    $phone = preg_replace('/[^0-9+]/', '', $phone);
    if (strpos($phone, '+') !== 0) {
        // Assume Bangladesh if no country code
        if (strpos($phone, '0') === 0) {
            $phone = '+88' . $phone;
        } else {
            $phone = '+880' . $phone;
        }
    }

    $customer_name = trim($order->get_billing_first_name() . ' ' . $order->get_billing_last_name());
    $order_total = $order->get_total();
    $order_id = $order->get_id();
    $items = [];
    
    foreach ($order->get_items() as $item) {
        $items[] = $item->get_name() . ' x' . $item->get_quantity();
    }
    $items_text = implode(', ', array_slice($items, 0, 3)); // Limit to 3 items in voice
    if (count($items) > 3) {
        $items_text .= ' সহ আরও পণ্য';
    }

    // Build the IVR voice message
    if ($language === 'bn-BD') {
        $voice_message = "আসসালামু আলাইকুম {$customer_name} ভাই। "
            . "আপনার অর্ডার নম্বর {$order_id} সফলভাবে রিসিভ হয়েছে। "
            . "আপনি অর্ডার করেছেন {$items_text}, মোট {$order_total} টাকা। "
            . "অর্ডার কনফার্ম করতে 1 চাপুন। "
            . "আমাদের সাথে কথা বলতে 2 চাপুন। "
            . "ক্যান্সেল করতে 3 চাপুন।";
    } else {
        $voice_message = "Hello {$customer_name}. "
            . "Your order number {$order_id} has been received. "
            . "You ordered {$items_text}, total {$order_total} taka. "
            . "Press 1 to confirm your order. "
            . "Press 2 to speak with us. "
            . "Press 3 to cancel.";
    }

    // Build webhook URL for delivery result
    $webhook_url = rest_url('manydial/v1/webhook');

    // Prepare form data (ManyDial uses multipart/form-data)
    $boundary = wp_generate_password(24, false);
    $body = '';

    $fields = [
        'callerId'       => $caller_id,
        'destination'    => $phone,
        'voiceMessage'   => $voice_message,
        'language'       => $language,
        'voice'          => $voice,
        'deliveryHook'   => $webhook_url,
        'payload'        => wp_json_encode([
            'orderId'     => $order_id,
            'source'      => 'woocommerce',
            'customerName' => $customer_name,
        ]),
    ];

    // Add forward number if configured
    if (!empty($forward_number)) {
        $fields['forwardNumber2'] = $forward_number;
    }

    foreach ($fields as $name => $value) {
        $body .= "--{$boundary}\r\n";
        $body .= "Content-Disposition: form-data; name=\"{$name}\"\r\n\r\n";
        $body .= "{$value}\r\n";
    }
    $body .= "--{$boundary}--\r\n";

    $response = wp_remote_post('https://api.manydial.com/v1/portal/call/dispatch', [
        'headers' => [
            'x-api-key'    => $api_key,
            'Content-Type' => 'multipart/form-data; boundary=' . $boundary,
        ],
        'body'    => $body,
        'timeout' => 30,
    ]);

    if (is_wp_error($response)) {
        return ['success' => false, 'error' => $response->get_error_message()];
    }

    $status_code = wp_remote_retrieve_response_code($response);
    $response_body = wp_remote_retrieve_body($response);
    $data = json_decode($response_body, true);

    if ($status_code >= 200 && $status_code < 300) {
        // Store call ID on the order for tracking
        $call_id = $data['callId'] ?? $data['id'] ?? '';
        if ($call_id) {
            $order->update_meta_data('_manydial_call_id', $call_id);
            $order->save();
        }
        return ['success' => true, 'data' => $data];
    } else {
        $error_msg = $data['message'] ?? $data['error'] ?? "HTTP {$status_code}";
        return ['success' => false, 'error' => $error_msg];
    }
}

// ============================================================
// 5. WEBHOOK — Receive Call Delivery Results
// ============================================================

add_action('rest_api_init', 'manydial_register_webhook');
function manydial_register_webhook() {
    register_rest_route('manydial/v1', '/webhook', [
        'methods'             => 'POST',
        'callback'            => 'manydial_handle_webhook',
        'permission_callback' => '__return_true', // Public endpoint for ManyDial callbacks
    ]);
}

function manydial_handle_webhook(WP_REST_Request $request) {
    $body = $request->get_json_params();
    
    if (empty($body)) {
        $body = $request->get_body_params();
    }

    // Log the webhook for debugging
    if (defined('WP_DEBUG') && WP_DEBUG) {
        error_log('ManyDial Webhook: ' . wp_json_encode($body));
    }

    $payload = $body['payload'] ?? '{}';
    if (is_string($payload)) {
        $payload = json_decode($payload, true);
    }

    $order_id = $payload['orderId'] ?? null;
    $dtmf_input = $body['dtmfInput'] ?? $body['customerInput'] ?? null;
    $call_status = $body['status'] ?? $body['callStatus'] ?? 'unknown';
    $call_duration = $body['duration'] ?? $body['callDuration'] ?? 0;

    if (!$order_id) {
        return new WP_REST_Response(['status' => 'ok', 'message' => 'No order ID'], 200);
    }

    $order = wc_get_order($order_id);
    if (!$order) {
        return new WP_REST_Response(['status' => 'ok', 'message' => 'Order not found'], 200);
    }

    // Process based on customer's DTMF input
    switch ($dtmf_input) {
        case '1': // Customer confirmed
            $order->update_status('processing', '✅ Customer confirmed order via ManyDial call (pressed 1)');
            $order->add_order_note(sprintf(
                '📞 ManyDial Call Result: Customer CONFIRMED (pressed 1). Duration: %ds. Status: %s',
                $call_duration,
                $call_status
            ));
            break;

        case '2': // Customer wants to talk — call was forwarded
            $order->add_order_note(sprintf(
                '📞 ManyDial Call Result: Customer requested callback (pressed 2, forwarded). Duration: %ds. Status: %s',
                $call_duration,
                $call_status
            ));
            break;

        case '3': // Customer cancelled
            $order->update_status('cancelled', '❌ Customer cancelled order via ManyDial call (pressed 3)');
            $order->add_order_note(sprintf(
                '📞 ManyDial Call Result: Customer CANCELLED (pressed 3). Duration: %ds. Status: %s',
                $call_duration,
                $call_status
            ));
            break;

        default: // No input or call not answered
            $order->add_order_note(sprintf(
                '📞 ManyDial Call Result: No response (DTMF: %s). Duration: %ds. Status: %s',
                $dtmf_input ?? 'none',
                $call_duration,
                $call_status
            ));
            break;
    }

    // Save call metadata
    $order->update_meta_data('_manydial_last_call_status', $call_status);
    $order->update_meta_data('_manydial_last_dtmf', $dtmf_input ?? 'none');
    $order->update_meta_data('_manydial_last_call_duration', $call_duration);
    $order->update_meta_data('_manydial_last_call_time', current_time('mysql'));
    $order->save();

    return new WP_REST_Response(['status' => 'ok', 'processed' => true], 200);
}

// ============================================================
// 6. ORDER LIST — Show Call Status Column
// ============================================================

add_filter('manage_edit-shop_order_columns', 'manydial_add_order_column');
function manydial_add_order_column($columns) {
    $new_columns = [];
    foreach ($columns as $key => $value) {
        $new_columns[$key] = $value;
        if ($key === 'order_status') {
            $new_columns['manydial_call'] = '📞 Call';
        }
    }
    return $new_columns;
}

add_action('manage_shop_order_posts_custom_column', 'manydial_render_order_column', 10, 2);
function manydial_render_order_column($column, $post_id) {
    if ($column !== 'manydial_call') return;

    $order = wc_get_order($post_id);
    if (!$order) return;

    $dtmf = $order->get_meta('_manydial_last_dtmf');
    $status = $order->get_meta('_manydial_last_call_status');

    if (empty($status) && empty($dtmf)) {
        echo '<span style="color:#999;">—</span>';
    } elseif ($dtmf === '1') {
        echo '<span style="color:green;" title="Confirmed">✅</span>';
    } elseif ($dtmf === '3') {
        echo '<span style="color:red;" title="Cancelled">❌</span>';
    } elseif ($dtmf === '2') {
        echo '<span style="color:orange;" title="Forwarded">📞</span>';
    } else {
        echo '<span style="color:#999;" title="No response">⏳</span>';
    }
}

// ============================================================
// 7. ORDER DETAILS — ManyDial Meta Box
// ============================================================

add_action('add_meta_boxes', 'manydial_add_order_meta_box');
function manydial_add_order_meta_box() {
    add_meta_box(
        'manydial_call_info',
        '📞 ManyDial Call Info',
        'manydial_order_meta_box_html',
        'shop_order',
        'side',
        'default'
    );
}

function manydial_order_meta_box_html($post) {
    $order = wc_get_order($post->ID);
    if (!$order) return;

    $call_id = $order->get_meta('_manydial_call_id');
    $status = $order->get_meta('_manydial_last_call_status');
    $dtmf = $order->get_meta('_manydial_last_dtmf');
    $duration = $order->get_meta('_manydial_last_call_duration');
    $time = $order->get_meta('_manydial_last_call_time');

    if (empty($status) && empty($call_id)) {
        echo '<p style="color:#999;">No call dispatched yet.</p>';
        echo '<p><em>Use "Order actions" dropdown → "ManyDial: Call Customer" to initiate a call.</em></p>';
        return;
    }

    echo '<table style="width:100%;font-size:13px;">';
    
    if ($call_id) {
        echo '<tr><td><strong>Call ID:</strong></td><td>' . esc_html($call_id) . '</td></tr>';
    }
    if ($status) {
        echo '<tr><td><strong>Status:</strong></td><td>' . esc_html($status) . '</td></tr>';
    }
    if ($dtmf) {
        $dtmf_label = match($dtmf) {
            '1' => '✅ Confirmed (1)',
            '2' => '📞 Forwarded (2)',
            '3' => '❌ Cancelled (3)',
            default => $dtmf,
        };
        echo '<tr><td><strong>Response:</strong></td><td>' . esc_html($dtmf_label) . '</td></tr>';
    }
    if ($duration) {
        echo '<tr><td><strong>Duration:</strong></td><td>' . esc_html($duration) . 's</td></tr>';
    }
    if ($time) {
        echo '<tr><td><strong>Last Call:</strong></td><td>' . esc_html($time) . '</td></tr>';
    }

    echo '</table>';
}

// ============================================================
// 8. ADMIN BAR — Quick Call Button (Order Edit Page)
// ============================================================

add_action('woocommerce_order_item_add_action_buttons', 'manydial_add_quick_call_button');
function manydial_add_quick_call_button($order) {
    $phone = $order->get_billing_phone();
    if (empty($phone)) return;

    $nonce = wp_create_nonce('manydial_quick_call_' . $order->get_id());
    ?>
    <button type="button" class="button button-primary" 
            onclick="manydialQuickCall(<?php echo $order->get_id(); ?>, '<?php echo esc_js($nonce); ?>')"
            id="manydial-quick-call-btn">
        📞 ManyDial Call
    </button>
    <script>
    function manydialQuickCall(orderId, nonce) {
        var btn = document.getElementById('manydial-quick-call-btn');
        btn.disabled = true;
        btn.textContent = '📞 Calling...';

        fetch(ajaxurl + '?action=manydial_quick_call&order_id=' + orderId + '&_wpnonce=' + nonce)
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    btn.textContent = '✅ Call Dispatched!';
                    btn.style.background = '#28a745';
                    setTimeout(() => {
                        btn.textContent = '📞 ManyDial Call';
                        btn.style.background = '';
                        btn.disabled = false;
                    }, 3000);
                } else {
                    btn.textContent = '❌ ' + data.data;
                    btn.style.background = '#dc3545';
                    setTimeout(() => {
                        btn.textContent = '📞 ManyDial Call';
                        btn.style.background = '';
                        btn.disabled = false;
                    }, 3000);
                }
            })
            .catch(() => {
                btn.textContent = '❌ Request failed';
                btn.disabled = false;
            });
    }
    </script>
    <?php
}

add_action('wp_ajax_manydial_quick_call', 'manydial_handle_quick_call');
function manydial_handle_quick_call() {
    $order_id = intval($_GET['order_id'] ?? 0);
    check_ajax_referer('manydial_quick_call_' . $order_id);

    $order = wc_get_order($order_id);
    if (!$order) {
        wp_send_json_error('Order not found');
        return;
    }

    $result = manydial_dispatch_call($order);
    if ($result['success']) {
        $order->add_order_note('✅ ManyDial quick call dispatched to ' . $order->get_billing_phone());
        wp_send_json_success('Call dispatched');
    } else {
        wp_send_json_error($result['error']);
    }
    wp_die();
}

// ============================================================
// 9. PLUGIN ACTIVATION & DEACTIVATION
// ============================================================

register_activation_hook(__FILE__, 'manydial_activate');
function manydial_activate() {
    // Set default options
    add_option('manydial_language', 'bn-BD');
    add_option('manydial_voice', 'female');
    add_option('manydial_auto_call', 'no');
    
    // Flush rewrite rules for REST API endpoint
    flush_rewrite_rules();
}

register_deactivation_hook(__FILE__, 'manydial_deactivate');
function manydial_deactivate() {
    flush_rewrite_rules();
}

// ============================================================
// 10. HPOS (High-Performance Order Storage) Compatibility
// ============================================================

add_action('before_woocommerce_init', function() {
    if (class_exists(\Automattic\WooCommerce\Utilities\FeaturesUtil::class)) {
        \Automattic\WooCommerce\Utilities\FeaturesUtil::declare_compatibility('custom_order_tables', __FILE__, true);
    }
});
