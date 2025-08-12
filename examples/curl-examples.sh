#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Marketplace Engine - cURL API Examples
# This script demonstrates common API operations using cURL

API_URL="http://localhost:5000/api/v1"
USER_ID="1"
ADMIN_ROLE="Administrator"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Marketplace Engine API Examples ===${NC}\n"

# ============================================================================
# LISTINGS EXAMPLES
# ============================================================================

echo -e "${BLUE}1. Create a Listing${NC}"
curl -X POST "$API_URL/listings" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: $USER_ID" \
  -d '{
    "title": "iPhone 14 Pro - 256GB",
    "description": "Excellent condition, barely used",
    "price": {
      "amount": 999.99,
      "currency": "USD"
    },
    "category": "Electronics",
    "tags": ["phone", "apple", "unlocked"],
    "location": {
      "city": "New York",
      "state": "NY",
      "country": "USA"
    }
  }' | jq .
echo -e "\n"

# ============================================================================
# Get All Listings
# ============================================================================

echo -e "${BLUE}2. Get All Listings${NC}"
curl -X GET "$API_URL/listings?page=1&pageSize=5" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Search Listings
# ============================================================================

echo -e "${BLUE}3. Search Listings${NC}"
curl -X GET "$API_URL/listings/search?q=iPhone&category=Electronics&priceMin=500&priceMax=1500" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Get Listing Details
# ============================================================================

echo -e "${BLUE}4. Get Listing Details (ID: 1)${NC}"
curl -X GET "$API_URL/listings/1" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Update Listing
# ============================================================================

echo -e "${BLUE}5. Update Listing${NC}"
curl -X PUT "$API_URL/listings/1" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: $USER_ID" \
  -d '{
    "title": "iPhone 14 Pro - Price Reduced!",
    "price": {
      "amount": 949.99,
      "currency": "USD"
    }
  }' | jq .
echo -e "\n"

# ============================================================================
# USERS EXAMPLES
# ============================================================================

echo -e "${BLUE}6. Register User${NC}"
curl -X POST "$API_URL/users/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "username": "john_doe",
    "fullName": "John Doe",
    "password": "SecurePassword123!"
  }' | jq .
echo -e "\n"

# ============================================================================
# Get User Profile
# ============================================================================

echo -e "${BLUE}7. Get User Profile${NC}"
curl -X GET "$API_URL/users/1" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Get Top Sellers
# ============================================================================

echo -e "${BLUE}8. Get Top Sellers${NC}"
curl -X GET "$API_URL/users/sellers/top?limit=5" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# MESSAGES EXAMPLES
# ============================================================================

echo -e "${BLUE}9. Send a Message${NC}"
curl -X POST "$API_URL/messages" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 2" \
  -d '{
    "recipientId": 1,
    "content": "Is this item still available?",
    "listingId": 1
  }' | jq .
echo -e "\n"

# ============================================================================
# Get Conversations
# ============================================================================

echo -e "${BLUE}10. Get User Conversations${NC}"
curl -X GET "$API_URL/messages/conversations?userId=1&page=1&pageSize=10" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# CATEGORIES EXAMPLES
# ============================================================================

echo -e "${BLUE}11. Get All Categories${NC}"
curl -X GET "$API_URL/categories" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Get Category Listings
# ============================================================================

echo -e "${BLUE}12. Get Category Listings${NC}"
curl -X GET "$API_URL/categories/1/listings?page=1&pageSize=10" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# Create Category (Admin Only)
# ============================================================================

echo -e "${BLUE}13. Create Category (Admin)${NC}"
curl -X POST "$API_URL/categories" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: $USER_ID" \
  -H "X-User-Role: $ADMIN_ROLE" \
  -d '{
    "name": "Computers",
    "description": "Desktop and laptop computers"
  }' | jq .
echo -e "\n"

# ============================================================================
# MODERATION EXAMPLES
# ============================================================================

echo -e "${BLUE}14. Create Moderation Report${NC}"
curl -X POST "$API_URL/moderation/reports" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{
    "reason": "Inappropriate content",
    "priority": "High",
    "targetListingId": 1,
    "description": "This listing contains inappropriate language"
  }' | jq .
echo -e "\n"

# ============================================================================
# Get Reports (Admin/Moderator)
# ============================================================================

echo -e "${BLUE}15. Get Moderation Reports${NC}"
curl -X GET "$API_URL/moderation/reports?status=Pending&priority=High" \
  -H "Accept: application/json" \
  -H "X-User-Id: $USER_ID" \
  -H "X-User-Role: $ADMIN_ROLE" | jq .
echo -e "\n"

# ============================================================================
# HEALTH CHECK
# ============================================================================

echo -e "${BLUE}16. Health Check${NC}"
curl -X GET "$API_URL/health" \
  -H "Accept: application/json" | jq .
echo -e "\n"

# ============================================================================
# NOTES
# ============================================================================

echo -e "${BLUE}=== Notes ===${NC}"
echo "- Replace localhost:5000 with your API URL"
echo "- Update X-User-Id and X-User-Role headers as needed"
echo "- Most endpoints return JSON responses"
echo "- Use 'jq' for pretty-printing JSON (install with: brew install jq)"
echo ""
echo -e "${GREEN}✓ API Examples Completed${NC}"
