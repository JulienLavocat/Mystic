#!/bin/bash

ctlptl apply -f cluster.yaml
helm --namespace platform upgrade --install  monitoring signoz/signoz --create-namespace

