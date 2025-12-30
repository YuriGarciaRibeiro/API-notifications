#!/bin/bash

# ========================================
# Build and Push Docker Images
# ========================================
# Usage: ./build-and-push.sh [version]
# Example: ./build-and-push.sh 1.0.0

set -e

# Configuration
VERSION=${1:-latest}
REGISTRY=${DOCKER_REGISTRY:-localhost}

echo "========================================="
echo "Building Notification System Images"
echo "Version: $VERSION"
echo "Registry: $REGISTRY"
echo "========================================="

# Navigate to project root
cd "$(dirname "$0")"

# Build API
echo ""
echo "Building API..."
docker build \
  -f src/NotificationSystem.Api/Dockerfile \
  -t ${REGISTRY}/notification-system-api:${VERSION} \
  -t ${REGISTRY}/notification-system-api:latest \
  .

# Build Email Consumer
echo ""
echo "Building Email Consumer..."
docker build \
  -f src/Consumers/NotificationSystem.Consumer.Email/Dockerfile \
  -t ${REGISTRY}/notification-system-consumer-email:${VERSION} \
  -t ${REGISTRY}/notification-system-consumer-email:latest \
  .

# Build SMS Consumer
echo ""
echo "Building SMS Consumer..."
docker build \
  -f src/Consumers/NotificationSystem.Consumer.Sms/Dockerfile \
  -t ${REGISTRY}/notification-system-consumer-sms:${VERSION} \
  -t ${REGISTRY}/notification-system-consumer-sms:latest \
  .

# Build Push Consumer
echo ""
echo "Building Push Consumer..."
docker build \
  -f src/Consumers/NotificationSystem.Consumer.Push/Dockerfile \
  -t ${REGISTRY}/notification-system-consumer-push:${VERSION} \
  -t ${REGISTRY}/notification-system-consumer-push:latest \
  .

echo ""
echo "========================================="
echo "Build Complete!"
echo "========================================="

# Push images if registry is not localhost
if [ "$REGISTRY" != "localhost" ]; then
  echo ""
  read -p "Push images to registry $REGISTRY? (y/n) " -n 1 -r
  echo
  if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Pushing images..."

    docker push ${REGISTRY}/notification-system-api:${VERSION}
    docker push ${REGISTRY}/notification-system-api:latest

    docker push ${REGISTRY}/notification-system-consumer-email:${VERSION}
    docker push ${REGISTRY}/notification-system-consumer-email:latest

    docker push ${REGISTRY}/notification-system-consumer-sms:${VERSION}
    docker push ${REGISTRY}/notification-system-consumer-sms:latest

    docker push ${REGISTRY}/notification-system-consumer-push:${VERSION}
    docker push ${REGISTRY}/notification-system-consumer-push:latest

    echo ""
    echo "========================================="
    echo "Push Complete!"
    echo "========================================="
  fi
fi

echo ""
echo "Images built:"
echo "  - ${REGISTRY}/notification-system-api:${VERSION}"
echo "  - ${REGISTRY}/notification-system-consumer-email:${VERSION}"
echo "  - ${REGISTRY}/notification-system-consumer-sms:${VERSION}"
echo "  - ${REGISTRY}/notification-system-consumer-push:${VERSION}"
