FROM cytopia/ansible:latest

ARG vault_password
ARG github_password

RUN apk add git
RUN apk add openssh
RUN apk add sshpass

RUN 'git' 'clone' 'https://kieronmacknight:$github_password@github.com/355gts/CovidCore.git'

WORKDIR /data/CovidCore/Deployment/openshift_ansible/

RUN echo "$vault_password" > vault_password

ENV ANSIBLE_HOST_KEY_CHECKING=false
