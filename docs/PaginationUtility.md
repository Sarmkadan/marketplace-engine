# PaginationUtility

A stateless utility class that provides helper methods and properties for calculating and validating pagination parameters such as page size, offsets, and page navigation.

## API

### `public static int CalculateOffset(int page, int pageSize)`

Calculates the zero-based offset for a given page and page size.

- **Parameters**
  - `page` – The one-based page number to calculate the offset for.
  - `pageSize` – The number of items per page.
- **Returns**
  - The zero-based offset for the specified page.
- **Throws**
  - `ArgumentOutOfRangeException` if `page` is less than 1 or `pageSize` is less than 1.

---

### `public static void ValidatePageParameters(int page, int pageSize, int totalItems)`

Validates that the provided page and page size are within acceptable bounds given the total number of items.

- **Parameters**
  - `page` – The one-based page number to validate.
  - `pageSize` – The number of items per page.
  - `totalItems` – The total number of items available.
- **Throws**
  - `ArgumentOutOfRangeException` if `page` is less than 1, `pageSize` is less than 1 or greater than the maximum allowed page size, or if `page` exceeds the total number of pages.

---

### `public static int CalculateTotalPages(int totalItems, int pageSize)`

Calculates the total number of pages available given the total items and page size.

- **Parameters**
  - `totalItems` – The total number of items to paginate.
  - `pageSize` – The number of items per page.
- **Returns**
  - The total number of pages, rounded up to the nearest integer.
- **Throws**
  - `ArgumentOutOfRangeException` if `totalItems` is negative or `pageSize` is less than 1.

---

### `public static bool HasNextPage(int currentPage, int totalPages)`

Determines whether there is a next page available after the current page.

- **Parameters**
  - `currentPage` – The one-based current page number.
  - `totalPages` – The total number of pages.
- **Returns**
  - `true` if `currentPage` is less than `totalPages`; otherwise, `false`.

---

### `public static bool HasPreviousPage(int currentPage)`

Determines whether there is a previous page available before the current page.

- **Parameters**
  - `currentPage` – The one-based current page number.
- **Returns**
  - `true` if `currentPage` is greater than 1; otherwise, `false`.

---

### `public static int? GetNextPage(int currentPage, int totalPages)`

Returns the one-based page number of the next page, if available.

- **Parameters**
  - `currentPage` – The one-based current page number.
  - `totalPages` – The total number of pages.
- **Returns**
  - The one-based page number of the next page, or `null` if there is no next page.

---

### `public static int? GetPreviousPage(int currentPage)`

Returns the one-based page number of the previous page, if available.

- **Parameters**
  - `currentPage` – The one-based current page number.
- **Returns**
  - The one-based page number of the previous page, or `null` if there is no previous page.

---

### `public static int GetDefaultPageSize()`

Returns the default page size used by the utility.

- **Returns**
  - The default page size as an integer.

---

### `public static int GetMaxPageSize()`

Returns the maximum allowed page size.

- **Returns**
  - The maximum page size as an integer.

---
### `public int CurrentPage`

Gets the one-based current page number.

- **Value**
  - The current page number.

---
### `public int PageSize`

Gets the number of items per page.

- **Value**
  - The page size.

---
### `public int TotalItems`

Gets the total number of items available.

- **Value**
  - The total number of items.

## Usage

### Example 1: Basic Pagination Setup
