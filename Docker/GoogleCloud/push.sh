cd ../..
docker build -f ./Docker/Dockerfile -t  europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes .
if [ "$?" == "0" ]; then
  docker push europe-west4-docker.pkg.dev/zestien3-web-site/recipes/recipes
else
  echo "docker build failed so docker push was skipped."
fi
