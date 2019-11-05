// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete reference for all Marketplace Engine API endpoints.

## Base URL

```
Development:  http://localhost:5000
Production:   https://api.marketplace.example.com
API Version:  v1
```

## Authentication

### Phase 1 (Current)
Use header-based identification:

```
X-User-Id: 1
X-User-Role: Administrator
```

### Phase 2
JWT Bearer token:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Response Format

### Success Response

All successful responses follow this structure:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": 1,
    "name": "Example"
  },
  "timestamp": "2026-05-04T10:30:00Z",
  "requestId": "abc-123-def"
}
```

### Error Response

```json
{
  "success": false,
  "message": "Detailed error description",
  "errorCode": "ERROR_CODE",
  "details": [
    "Additional context 1",
    "Additional context 2"
  ],
  "timestamp": "2026-05-04T10:30:00Z",
  "requestId": "abc-123-def"
}
```

### HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request succeeded |
| 201 | Created - Resource created successfully |
| 204 | No Content - Success with no response body |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Resource already exists |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |

---

## Listings Endpoints

### List Listings

```http
GET /api/v1/listings?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

**Query Parameters:**
- `page` (integer, optional, default: 1) - Page number
- `pageSize` (integer, optional, default: 20, max: 100) - Items per page
- `sortBy` (string, optional) - Sort field: `createdAt`, `price`, `rating`
- `sortOrder` (string, optional) - `asc` or `desc`

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "iPhone 14 Pro",
        "description": "Excellent condition...",
        "price": {
          "amount": 999.99,
          "currency": "USD"
        },
        "sellerId": 5,
        "status": "Active",
        "tags": ["phone", "apple"],
        "location": {
          "city": "New York",
          "country": "USA"
        },
        "createdAt": "2026-05-01T10:00:00Z",
        "updatedAt": "2026-05-03T15:30:00Z"
      }
    ],
    "totalCount": 250,
    "pageSize": 20,
    "currentPage": 1,
    "totalPages": 13
  }
}
```

### Get Listing Details

```http
GET /api/v1/listings/{id}
```

**Path Parameters:**
- `id` (integer, required) - Listing ID

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "iPhone 14 Pro",
    "description": "Original box, minimal use, factory settings reset",
    "price": { "amount": 999.99, "currency": "USD" },
    "sellerId": 5,
    "sellerName": "John Seller",
    "sellerRating": 4.8,
    "status": "Active",
    "category": "Electronics",
    "tags": ["phone", "apple", "unlocked"],
    "images": [
      "https://example.com/image1.jpg",
      "https://example.com/image2.jpg"
    ],
    "location": {
      "city": "New York",
      "state": "NY",
      "country": "USA"
    },
    "isFeatured": false,
    "viewCount": 156,
    "createdAt": "2026-05-01T10:00:00Z",
    "updatedAt": "2026-05-03T15:30:00Z"
  }
}
```

**Error Responses:**
- 404 Not Found - Listing doesn't exist

### Search Listings (prefix/keyword)

```http
GET /api/v1/listings/search?q=iphone&limit=10
```

**Query Parameters:**
- `q` (string, required, min: 2 chars) - Search query matched against title and description
- `limit` (integer, optional, default: 10) - Maximum results to return

**Response (200 OK):**
```json
{
  "query": "iphone",
  "results": [
    {
      "id": "3fa85f64-...",
      "title": "iPhone 14 Pro - 256GB Space Black",
      "price": 999.99,
      "categoryId": "...",
      "status": "Active"
    }
  ]
}
```

---

## Full-Text Search Endpoints

### Full-Text Search with Relevance Scoring

```http
GET /api/v1/search/full-text?q=vintage+leather+jacket&categoryId=...&minPrice=50&maxPrice=500&page=1&pageSize=20
```

Full-text search across listing titles, descriptions, and tags with ranked relevance scoring.
Results are ordered by a TF-inspired relevance score that also boosts featured listings and
recently published items. The response includes aggregated facets for further filtering.

**Query Parameters:**
- `q` (string, required) - Free-text query; supports multi-word phrases
- `categoryId` (UUID, optional) - Restrict results to a specific category
- `minPrice` (decimal, optional) - Minimum price filter
- `maxPrice` (decimal, optional) - Maximum price filter
- `condition` (string, optional) - Filter by condition (e.g. `new`, `used`)
- `featuredOnly` (boolean, optional) - Restrict to featured listings
- `page` (integer, optional, default: 1) - Page number
- `pageSize` (integer, optional, default: 20, max: 100) - Results per page

**Response (200 OK):**
```json
{
  "query": "vintage leather jacket",
  "totalHits": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "hits": [
    {
      "id": "3fa85f64-...",
      "title": "Vintage Leather Jacket - 1970s Biker Style",
      "description": "Genuine leather, worn-in patina...",
      "price": 149.99,
      "categoryId": "...",
      "relevanceScore": 0.97,
      "matchedFields": ["title", "description", "tags"]
    }
  ],
  "facets": [
    {
      "name": "Price Range",
      "field": "price",
      "values": [
        { "label": "$100 – $500", "value": "100-500", "count": 28 }
      ]
    },
    {
      "name": "Condition",
      "field": "condition",
      "values": [
        { "label": "Used", "value": "used", "count": 35 },
        { "label": "New", "value": "new", "count": 7 }
      ]
    }
  ],
  "elapsedMilliseconds": 12
}
```

### Search Autocomplete Suggestions

```http
GET /api/v1/search/suggestions?prefix=vint&limit=10
```

Returns distinct listing titles that begin with the given prefix, ordered by featured status
and view count for relevance.

**Query Parameters:**
- `prefix` (string, required, min: 2 chars) - Partial query string to complete
- `limit` (integer, optional, default: 10, max: 50) - Maximum suggestions

**Response (200 OK):**
```json
{
  "prefix": "vint",
  "suggestions": [
    "Vintage Leather Jacket",
    "Vintage Denim Jacket",
    "Vintage Camera"
  ]
}
```

### Create Listing

```http
POST /api/v1/listings
Content-Type: application/json
X-User-Id: 1
```

**Request Body:**
```json
{
  "title": "iPhone 14 Pro",
  "description": "Excellent condition, minimal use",
  "price": {
    "amount": 999.99,
    "currency": "USD"
  },
  "category": "Electronics",
  "tags": ["phone", "apple", "unlocked"],
  "images": [
    "https://example.com/image1.jpg"
  ],
  "location": {
    "city": "New York",
    "state": "NY",
    "country": "USA"
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Listing created successfully",
  "data": {
    "id": 101,
    "title": "iPhone 14 Pro",
    "status": "Active"
  }
}
```

**Error Responses:**
- 400 Bad Request - Invalid input
- 401 Unauthorized - User not authenticated
- 409 Conflict - Listing already exists

### Update Listing

```http
PUT /api/v1/listings/{id}
Content-Type: application/json
X-User-Id: 1
```

**Request Body (partial update):**
```json
{
  "title": "iPhone 14 Pro - Updated",
  "price": {
    "amount": 949.99,
    "currency": "USD"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Listing updated successfully",
  "data": {
    "id": 1,
    "title": "iPhone 14 Pro - Updated"
  }
}
```

### Delete Listing

```http
DELETE /api/v1/listings/{id}
X-User-Id: 1
```

**Response (204 No Content):**
```
[Empty response body]
```

### Archive Listing

```http
PATCH /api/v1/listings/{id}/archive
X-User-Id: 1
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Listing archived",
  "data": {
    "id": 1,
    "status": "Archived"
  }
}
```

---

## Users Endpoints

### Get User Profile

```http
GET /api/v1/users/{id}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "email": "seller@example.com",
    "username": "john_seller",
    "fullName": "John Smith",
    "role": "Premium Seller",
    "location": {
      "city": "New York",
      "country": "USA"
    },
    "rating": {
      "averageRating": 4.8,
      "totalReviews": 42,
      "positiveCount": 40,
      "negativeCount": 2
    },
    "isVerified": true,
    "joinedAt": "2025-01-15T10:00:00Z",
    "lastActivityAt": "2026-05-04T09:30:00Z"
  }
}
```

### Get Top Sellers

```http
GET /api/v1/users/sellers/top?limit=10
```

**Query Parameters:**
- `limit` (integer, optional, default: 10) - Number of sellers

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "sellers": [
      {
        "id": 5,
        "username": "john_seller",
        "rating": 4.9,
        "reviewCount": 128,
        "listingCount": 45
      }
    ]
  }
}
```

### Register User

```http
POST /api/v1/users/register
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "fullName": "John Doe",
  "password": "SecurePassword123!",
  "location": {
    "city": "New York",
    "country": "USA"
  }
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 100,
    "email": "user@example.com",
    "username": "johndoe"
  }
}
```

---

## Messages Endpoints

### Get Conversations

```http
GET /api/v1/messages/conversations?userId=1&page=1&pageSize=20
```

**Query Parameters:**
- `userId` (integer, required) - User ID
- `page` (integer, optional) - Page number
- `pageSize` (integer, optional) - Items per page

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "conversations": [
      {
        "id": 1,
        "participantId": 2,
        "participantName": "Jane Buyer",
        "lastMessage": "Is this still available?",
        "lastMessageAt": "2026-05-04T08:15:00Z",
        "unreadCount": 2,
        "listingId": 50
      }
    ],
    "totalCount": 15
  }
}
```

### Send Message

```http
POST /api/v1/messages
Content-Type: application/json
X-User-Id: 1
```

**Request Body:**
```json
{
  "recipientId": 2,
  "content": "Is this item still available?",
  "listingId": 50
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Message sent",
  "data": {
    "id": 501,
    "senderId": 1,
    "recipientId": 2,
    "content": "Is this item still available?",
    "createdAt": "2026-05-04T10:30:00Z"
  }
}
```

### Get Conversation Messages

```http
GET /api/v1/messages/conversations/{conversationId}/messages?after=<cursorId>&pageSize=50
```

Retrieves messages using **cursor-based pagination**. Unlike offset pagination, this approach
is stable under concurrent writes — new messages inserted between requests do not cause
duplicates or gaps.

**Query Parameters:**
- `after` (UUID, optional) — ID of the last message seen; omit on the first request
- `pageSize` (integer, optional, default: 50, max: 100) — Messages per page

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "3fa85f64-...",
      "senderId": "...",
      "senderName": "John Seller",
      "body": "Yes, still available!",
      "isRead": true,
      "createdAt": "2026-05-04T10:30:00Z"
    }
  ],
  "nextCursor": "7b3a9c12-...",
  "pageSize": 50,
  "hasMore": true
}
```

To fetch the next page, pass `nextCursor` as the `after` parameter. When `hasMore` is
`false` there are no further messages.

### Mark Message as Read

```http
PATCH /api/v1/messages/{id}/read
X-User-Id: 1
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Message marked as read"
}
```

---

## Categories Endpoints

### List All Categories

```http
GET /api/v1/categories?depth=1
```

The category system supports multi-level hierarchies (e.g. **Electronics > Phones > Smartphones**).
By default `GET /categories` returns only **root-level** categories (those with no parent).
Use the `?depth=N` parameter to include nested subcategories up to `N` levels deep.

**Query Parameters:**
- `depth` (integer, optional, default: 1) — How many levels of subcategories to include.
  - `depth=1` (default): root categories only, `subCategories` list is empty.
  - `depth=2`: root categories and their direct children.
  - `depth=3`: root, children, and grandchildren (e.g. Electronics > Phones > Smartphones).

**Category tree structure:**
```
Electronics  (root, parentCategoryId: null)
├── Phones   (parentCategoryId: Electronics.id)
│   ├── Smartphones
│   └── Feature Phones
└── Laptops
```

**Listing inheritance:** Listings belong to exactly one category. When filtering by a parent
category (e.g. `Electronics`), search results include listings from all descendant categories
unless a more specific category is requested.

**Search filters:** Use the `categoryId` parameter on search endpoints to restrict results to
a specific category node. To search across an entire branch, supply the parent category ID.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "categories": [
      {
        "id": 1,
        "name": "Electronics",
        "description": "Electronic devices and accessories",
        "parentCategoryId": null,
        "listingCount": 256,
        "isActive": true,
        "subCategories": [
          {
            "id": 2,
            "name": "Phones",
            "parentCategoryId": 1,
            "listingCount": 89,
            "subCategories": [
              {
                "id": 7,
                "name": "Smartphones",
                "parentCategoryId": 2,
                "listingCount": 72
              }
            ]
          }
        ]
      }
    ]
  }
}
```

### Get Category Listings

```http
GET /api/v1/categories/{id}/listings?page=1&pageSize=20
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "categoryName": "Electronics",
    "listings": [
      {
        "id": 1,
        "title": "iPhone 14 Pro",
        "price": { "amount": 999.99, "currency": "USD" }
      }
    ],
    "totalCount": 256
  }
}
```

### Create Category (Admin)

```http
POST /api/v1/categories
Content-Type: application/json
X-User-Id: 1
X-User-Role: Administrator
```

**Request Body:**
```json
{
  "name": "Electronics",
  "description": "Electronic devices",
  "parentCategoryId": null
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Electronics"
  }
}
```

---

## Moderation Endpoints

### Create Report

```http
POST /api/v1/moderation/reports
Content-Type: application/json
X-User-Id: 1
```

**Request Body:**
```json
{
  "reason": "Inappropriate content - offensive language",
  "priority": "High",
  "targetListingId": 456,
  "description": "Listing contains offensive terms"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 1001,
    "status": "Pending",
    "priority": "High",
    "createdAt": "2026-05-04T10:30:00Z"
  }
}
```

**Allowed Priorities:** `Low`, `Medium`, `High`, `Critical`

### Get Reports (Admin/Moderator)

```http
GET /api/v1/moderation/reports?status=Pending&priority=High&page=1
X-User-Id: 1
X-User-Role: Moderator
```

**Query Parameters:**
- `status` (string, optional) - `Pending`, `In Review`, `Approved`, `Rejected`
- `priority` (string, optional)
- `assignedTo` (integer, optional) - Moderator ID
- `page` (integer, optional)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "reports": [
      {
        "id": 1001,
        "reporterId": 1,
        "reason": "Inappropriate content",
        "priority": "High",
        "status": "Pending",
        "targetListingId": 456,
        "createdAt": "2026-05-04T10:30:00Z"
      }
    ],
    "totalCount": 12
  }
}
```

### Update Report Status (Moderator)

```http
PATCH /api/v1/moderation/reports/{id}
Content-Type: application/json
X-User-Id: 1
X-User-Role: Moderator
```

**Request Body:**
```json
{
  "status": "Approved",
  "action": "Remove listing",
  "notes": "Content violates community guidelines"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Report updated",
  "data": {
    "id": 1001,
    "status": "Approved"
  }
}
```

### Bulk Moderate Listings (Moderator/Admin)

```http
POST /api/v1/moderation/bulk
Content-Type: application/json
X-User-Role: Moderator
```

Applies a moderation action to multiple listings in a single request. Each listing is
processed independently and the response includes per-item success/failure status so
partial failures can be retried. Restricted to `Moderator` and `Administrator` roles.

**Request Body:**
```json
{
  "listingIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "7b3a9c12-1234-5678-9abc-def012345678"
  ],
  "action": "remove"
}
```

**Allowed Actions:**
- `approve` — Restores listing to Active status
- `remove` — Flags listing as removed
- `escalate` — Marks listing for escalated review

**Response (200 OK):**
```json
{
  "results": [
    {
      "listingId": "3fa85f64-...",
      "success": true,
      "error": null
    },
    {
      "listingId": "7b3a9c12-...",
      "success": false,
      "error": "Listing not found"
    }
  ],
  "successCount": 1,
  "failureCount": 1
}
```

**Error Responses:**
- 400 Bad Request — No listing IDs provided or missing/invalid action
- 403 Forbidden — Caller does not have Moderator or Administrator role

---

## Health Check

### Service Health

```http
GET /api/v1/health
```

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2026-05-04T10:30:00Z",
  "uptime": "2 days 5 hours"
}
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse.

**Default Limits (Phase 1):**
- 60 requests per minute per user

**Headers:**
```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1620000000
```

**When Exceeded (429):**
```json
{
  "success": false,
  "message": "Rate limit exceeded",
  "errorCode": "RATE_LIMIT_EXCEEDED",
  "details": [
    "Limit: 60 requests per minute",
    "Try again in 23 seconds"
  ]
}
```

---

## Error Codes Reference

| Error Code | HTTP Status | Description |
|-----------|-------------|-------------|
| `VALIDATION_FAILED` | 400 | Input validation error |
| `INVALID_CREDENTIALS` | 401 | Authentication failed |
| `INSUFFICIENT_PERMISSIONS` | 403 | User lacks required role |
| `RESOURCE_NOT_FOUND` | 404 | Resource doesn't exist |
| `RESOURCE_ALREADY_EXISTS` | 409 | Duplicate resource |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Server error |

---

**Next Steps:**
- See [Deployment Guide](deployment.md) for production setup
- Check [FAQ](faq.md) for common questions
- Review [Architecture](architecture.md) for system design
