BestRoute – Google Maps API Setup (Technical Steps)
===================================================

Purpose
-------

Enable **route optimization** and **Google Maps route link generation** using Google Maps **Directions API**, with **billing enabled** and **cost hard-controlled**.

* * *

1\. Create Google Cloud Project
-------------------------------

URL:  
https://console.cloud.google.com/projectcreate

Action:

*   Create a new project (any name)

* * *

2\. Enable Billing (required, not charging)
-------------------------------------------

URL:  
https://console.cloud.google.com/billing

Action:

*   Create or select a billing account
*   Attach it to the project

Notes:

*   Billing enabled ≠ charged
*   Required to use Maps APIs at all

* * *

3\. Google Maps Platform Onboarding (checkbox screen)
-----------------------------------------------------

Select **only**:

*   ✔ Add API key
*   ✔ Get directions and trip planning

Unselect **everything else**.

Proceed.

* * *

4\. Platform / Framework Selection
----------------------------------

When asked for framework/platform:

Select:

*   **Other**

When prompted to describe “Other”, enter:

```
Native Windows desktop application using Google Maps REST APIs
```

Proceed.

* * *

5\. Enable APIs
---------------

Enable **only** the required API.

### Required

*   **Directions API**  
    https://console.cloud.google.com/apis/library/directions-backend.googleapis.com

### Do NOT enable

*   Maps SDKs
*   Maps JavaScript API
*   Distance Matrix
*   Places
*   Embed
*   Elevation
*   Anything unrelated

* * *

6\. Create API Key
------------------

URL:  
[https://console.cloud.google.com/apis/credentials](https://console.cloud.google.com/apis/credentials)

Action:

*   Create credentials → API key

* * *

7\. Restrict API Key
--------------------

In API key settings:

### Application restriction

*   **None** (desktop app, no fixed origin)

### API restriction

*   ✔ Restrict key
*   ✔ Allow **Directions API**

Save.

* * *

8\. Quotas (Hard Cost Control)
------------------------------

Open quotas page:  
https://console.cloud.google.com/apis/api/directions-backend.googleapis.com/quotas

Set:

*   **Requests per day** → `1000`
*   **Requests per minute** → `10` (or similarly low)

Ignore:

*   Premium plan quotas
*   Per-user quotas
*   JavaScript / SDK quotas

Only the above two matter.

* * *

9\. EEA / Terms Confirmation
----------------------------
https://console.cloud.google.com/apis/library/directions-backend.googleapis.com?hl=en-GB

When prompted:

*   Type `Confirm`
*   Accept & continue

* * *

10\. What Service Is Being Used (clarity)
-----------------------------------------

Service:

*   **Google Maps – Directions API (Web Service)**

Mechanism:

*   App sends waypoints with `optimize:true`
*   API returns `waypoint_order`
*   App reorders addresses programmatically
*   App generates a **Google Maps Directions URL**
*   URL is the **only output**

No separate “link service” exists.

* * *

11\. Cost Model Summary
-----------------------

*   Free tier: **$200/month**, resets monthly
*   Directions API cost ≈ **$5 / 1,000 requests**
*   With 1,000/day cap → ~30,000/month → **$0**
*   Charges only occur if both:
    *   billing enabled
    *   quotas exceeded

* * *

End State Checklist
-------------------

*   Project created ✔
*   Billing enabled ✔
*   Directions API enabled ✔
*   API key created ✔
*   Key restricted ✔
*   Quotas set ✔
*   Terms accepted ✔

Setup complete.
