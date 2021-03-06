# This script distributes the built framework dll
# and it's dependencies all over my other unisave projects
# 
# This file thus works only on my PC and not anywhere else.

##############
# Distribute #
##############

VERSION=$(grep -oP "AssemblyInformationalVersion\(\"\K[^\"]+" UnisaveFramework/Properties/AssemblyInfo.cs)

# releases
echo "Copying to releases..."
mkdir -p releases/$VERSION
cp -R UnisaveFramework/bin/Debug/* releases/$VERSION
echo $VERSION > releases/latest.txt

# unity asset
echo "Copying to the asset..."
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.dll ../asset/Assets/Unisave/Libraries/UnisaveFramework
cp -R UnisaveFramework/bin/Debug/UnisaveFramework.pdb ../asset/Assets/Unisave/Libraries/UnisaveFramework

# restart sandboxes, to pull the new .dll file
echo "Restarting the script runner..."
echo "TODO: restart OpenFaas sandboxes"
exit 1
#docker restart script_runner > /dev/null

# done
echo "Done."
