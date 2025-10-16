#!/bin/bash

# Deploy to GitHub Pages
# This script builds the project and updates the base href for GitHub Pages

echo "🔨 Building production release..."
rm -rf publish
dotnet publish -c Release -o publish --nologo

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "📝 Updating base href for GitHub Pages..."
# Update the base href in the published index.html
sed -i '' 's|<base href="/" />|<base href="/MD-conversion-tool/" />|g' publish/wwwroot/index.html

echo "📦 Copying to docs folder..."
rm -rf docs
cp -r publish/wwwroot docs
touch docs/.nojekyll

echo "✅ Build complete! Files ready in docs/ folder"
echo ""
echo "Next steps:"
echo "  git add docs"
echo "  git commit -m 'Deploy to GitHub Pages'"
echo "  git push origin main"
