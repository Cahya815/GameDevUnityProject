terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region                      = "us-east-1"
  access_key                  = "test"
  secret_key                  = "test"

  # KUNCI UTAMA SUPAYA FLOCI GAK HANG:
  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true

  endpoints {
    ec2       = "http://localhost:4566"    # UNTUK EC2 INSTANCE
    s3        = "http://localhost:4566"
    dynamodb  = "http://localhost:4566"
    sqs       = "http://localhost:4566"
  }
}

# ─── PHASE 1: EC2 COMPUTE ──────────────────────────────────────────────

# Security Group untuk allow SSH (port 22) dan HTTP (port 8080)
resource "aws_security_group" "game_server_sg" {
  name        = "game-server-security-group"
  description = "Allow SSH and HTTP traffic"

  ingress {
    description = "SSH from anywhere"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTP from anywhere"
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# EC2 Instance (t2.micro Linux)
resource "aws_instance" "game_server" {
  ami                    = "ami-12345678"  # FLOCI akan accept dummy AMI
  instance_type          = "t2.micro"
  vpc_security_group_ids = [aws_security_group.game_server_sg.id]

  tags = {
    Name = "GamePemadam-Server"
  }

  # FLOCI dummy parameters
  user_data = <<-EOF
              #!/bin/bash
              echo "GamePemadam Server Initialized"
              echo "Backend port: 8080"
              EOF
}

# ─── S3 (Backup/Later Phase) ───────────────────────────────────────────
resource "aws_s3_bucket" "game_saves" {
  bucket = "my-game-saves"
}
