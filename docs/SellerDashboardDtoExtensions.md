# SellerDashboardDtoExtensions

Provides extension methods for `SellerDashboardDto` to compute derived metrics and format display values for seller dashboard views.

## API

### `GetConversionRate(SellerDashboardDto seller)`
Calculates the seller’s conversion rate as the ratio of successful orders to total orders.
- **Parameters**
  - `seller`: The `SellerDashboardDto` instance containing order metrics.
- **Return value**
  - A `double` representing the conversion rate (0.0 to 1.0).
- **Exceptions**
  - Throws `ArgumentNullException` if `seller` is `null`.
  - Throws `DivideByZeroException` if `seller.TotalOrders` is zero.

---

### `IsSellerActive(SellerDashboardDto seller)`
Determines whether the seller is currently active based on order activity and account status.
- **Parameters**
  - `seller`: The `SellerDashboardDto` instance to evaluate.
- **Return value**
  - `true` if the seller has recent activity or an active account; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `seller` is `null`.

---
### `GetFinancialSummary(SellerDashboardDto seller)`
Generates a formatted summary of the seller’s financial performance for display.
- **Parameters**
  - `seller`: The `SellerDashboardDto` instance containing financial data.
- **Return value**
  - A `string` summarizing earnings, payouts, and pending balances in a human-readable format.
- **Exceptions**
  - Throws `ArgumentNullException` if `seller` is `null`.

---
### `GetRatingDisplayText(SellerDashboardDto seller)`
Converts the seller’s numeric rating into a localized display string (e.g., "Excellent" or "Poor").
- **Parameters**
  - `seller`: The `SellerDashboardDto` instance containing rating data.
- **Return value**
  - A `string` representing the localized rating category.
- **Exceptions**
  - Throws `ArgumentNullException` if `seller` is `null`.

## Usage
