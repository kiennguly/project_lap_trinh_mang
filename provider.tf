terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "6.22.1"
    }
  }

  required_version = ">= 1.5.0"
}

provider "aws" {
  region = "ap-southeast-2" // muốn deloy lên đâu thì tự chỉnh nhé ahihiiiii
}

