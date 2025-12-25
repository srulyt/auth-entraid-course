#!/bin/bash
# Publish script for Labs.Cli
# This script publishes the CLI tool to the ./Labs.Cli/publish directory

echo -e "\033[0;36mPublishing Labs.Cli...\033[0m"

# Navigate to the Labs.Cli directory
cd "$(dirname "$0")/Labs.Cli"

# Clean previous publish output
if [ -d "./publish" ]; then
    rm -rf ./publish
    echo -e "\033[0;33mCleaned previous publish output\033[0m"
fi

# Publish the application
dotnet publish -c Release -o ./publish

if [ $? -eq 0 ]; then
    echo -e "\n\033[0;32mPublish successful!\033[0m"
    echo -e "\n\033[0;33mPublished to: $(pwd)/publish\033[0m"
    echo -e "\n\033[0;36mTo add the CLI to your PATH for this terminal session:\033[0m"
    echo -e "  \033[0;37mcd ..\033[0m"
    echo -e "  \033[0;37mexport PATH=\"\$(pwd)/Labs.Cli/publish:\$PATH\"\033[0m"
    echo -e "\n\033[0;36mOr if you prefer to stay in this directory:\033[0m"
    echo -e "  \033[0;37mexport PATH=\"\$(pwd)/publish:\$PATH\"\033[0m"
    echo -e "\n\033[0;36mThen you can use:\033[0m"
    echo -e "  \033[0;37mentra-lab --help\033[0m"
    echo -e "  \033[0;37mentra-lab login --mode pkce\033[0m"
else
    echo -e "\n\033[0;31mPublish failed!\033[0m"
    exit 1
fi
