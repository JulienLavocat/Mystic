dns:
	echo "127.0.0.1 monitoring.mystic.local" | sudo tee -a /etc/hosts >/dev/null

dev:
	ctlptl apply -f local/cluster.yaml
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
	sleep 10
	kubectl wait --namespace ingress-nginx \
  		--for=condition=ready pod \
  		--selector=app.kubernetes.io/component=controller \
  		--timeout=90s
	helm --namespace platform upgrade --install --atomic --timeout 300s monitoring signoz/signoz --create-namespace -f local/signoz.yaml
	helm --namespace platform upgrade --install --atomic --timeout 300s infra signoz/k8s-infra -f local/signoz-infra.yaml
