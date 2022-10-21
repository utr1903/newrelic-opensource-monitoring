#!/bin/bash

terraform -chdir=../terraform/01_platform_eks init

terraform -chdir=../terraform/01_platform_eks plan

terraform -chdir=../terraform/01_platform_eks apply
