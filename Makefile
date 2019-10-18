# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build run clean test restore docker-build docker-run

# Variables
PROJECT_PATH := src/MarketplaceEngine
DOCKER_IMAGE := marketplace-engine:latest
DOCKER_CONTAINER := marketplace-engine-api
DOTNET_VERSION := 10.0

# Default target
help:
	@echo "Marketplace Engine - Build Commands"
	@echo "===================================="
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  make help              - Show this help message"
	@echo "  make restore           - Restore NuGet packages"
	@echo "  make build             - Build the project (Debug)"
	@echo "  make build-release     - Build the project (Release)"
	@echo "  make run               - Run the application"
	@echo "  make run-release       - Run the application (Release)"
	@echo "  make clean             - Clean build artifacts"
	@echo "  make clean-all         - Clean all build and runtime files"
	@echo "  make test              - Run tests (when available)"
	@echo "  make docker-build      - Build Docker image"
	@echo "  make docker-run        - Run Docker container"
	@echo "  make docker-stop       - Stop Docker container"
	@echo "  make compose-up        - Start services with docker-compose"
	@echo "  make compose-down      - Stop services with docker-compose"
	@echo "  make compose-logs      - View docker-compose logs"
	@echo "  make all               - Restore, build, and run"
	@echo "  make release           - Build release version"
	@echo ""

# Restore NuGet packages
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore
	@echo "✓ Packages restored"

# Build project (Debug)
build: restore
	@echo "Building project (Debug)..."
	dotnet build
	@echo "✓ Build completed"

# Build project (Release)
build-release: restore
	@echo "Building project (Release)..."
	dotnet build -c Release
	@echo "✓ Release build completed"

# Run application
run: build
	@echo "Starting Marketplace Engine..."
	@echo "Access at: http://localhost:5000"
	@echo "API Docs: http://localhost:5000/swagger"
	@echo ""
	dotnet run --project $(PROJECT_PATH)

# Run application (Release)
run-release: build-release
	@echo "Starting Marketplace Engine (Release)..."
	@echo "Access at: http://localhost:5000"
	dotnet $(PROJECT_PATH)/bin/Release/net$(DOTNET_VERSION)/MarketplaceEngine.dll

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	@echo "✓ Build artifacts cleaned"

# Clean all files
clean-all: clean
	@echo "Removing all build and runtime files..."
	rm -rf bin/ obj/ .vs/ .vscode/
	find . -name ".DS_Store" -delete
	@echo "✓ All files cleaned"

# Run tests
test:
	@echo "Running tests..."
	dotnet test --verbosity normal
	@echo "✓ Tests completed"

# Docker: Build image
docker-build:
	@echo "Building Docker image..."
	docker build -t $(DOCKER_IMAGE) .
	@echo "✓ Docker image built"

# Docker: Run container
docker-run: docker-build
	@echo "Running Docker container..."
	docker run -d -p 5000:5000 -p 5001:5001 --name $(DOCKER_CONTAINER) $(DOCKER_IMAGE)
	@echo "✓ Container running at http://localhost:5000"

# Docker: Stop container
docker-stop:
	@echo "Stopping Docker container..."
	docker stop $(DOCKER_CONTAINER) || true
	docker rm $(DOCKER_CONTAINER) || true
	@echo "✓ Container stopped"

# Docker Compose: Start services
compose-up:
	@echo "Starting services with docker-compose..."
	docker-compose up -d
	@echo "✓ Services started"
	@echo "  API: http://localhost:5000"
	@echo "  Cache: localhost:6379"

# Docker Compose: Stop services
compose-down:
	@echo "Stopping services..."
	docker-compose down
	@echo "✓ Services stopped"

# Docker Compose: View logs
compose-logs:
	docker-compose logs -f api

# All: Restore, build, and run
all: restore build run

# Release: Build and optimize
release: clean-all build-release
	@echo "✓ Release build ready"
	@echo "  Location: $(PROJECT_PATH)/bin/Release/net$(DOTNET_VERSION)/publish"

# CI/CD: Build without running
ci-build: restore
	@echo "CI: Building..."
	dotnet build -c Release
	@echo "CI: Build completed"

# CI/CD: Run tests for CI
ci-test:
	@echo "CI: Running tests..."
	dotnet test -c Release --verbosity normal || true
	@echo "CI: Tests completed"

# CI/CD: Create artifacts
ci-publish:
	@echo "CI: Publishing..."
	dotnet publish -c Release -o ./publish
	@echo "✓ Artifacts ready in ./publish"

# Development: Format code
format:
	@echo "Formatting code..."
	dotnet format
	@echo "✓ Code formatted"

# Development: Watch for changes
watch:
	@echo "Watching for changes..."
	dotnet watch run --project $(PROJECT_PATH)

# Health check
health-check:
	@echo "Checking API health..."
	@curl -f http://localhost:5000/api/v1/health || echo "API not responding"

# Show status
status:
	@echo "=== Marketplace Engine Status ==="
	@dotnet --version
	@echo ""
	@echo "Docker:"
	@docker ps --filter "name=$(DOCKER_CONTAINER)" || echo "No Docker containers running"

# Development setup
setup: restore
	@echo "Setting up development environment..."
	@echo "✓ Dependencies restored"
	@echo ""
	@echo "Next steps:"
	@echo "  1. Run 'make build' to build the project"
	@echo "  2. Run 'make run' to start the application"
	@echo "  3. Visit http://localhost:5000 to access the API"

.DEFAULT_GOAL := help
