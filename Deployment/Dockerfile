FROM cytopia/ansible:latest

RUN addgroup -S covid && adduser -S covid -G covid 

ARG vault_password
ARG github_password

RUN apk add git
RUN apk add openssh
RUN apk add sshpass

#RUN echo "covid    ALL=(ALL) ALL" >> /etc/sudoers

RUN chown -Rf covid:covid /data/

USER covid

RUN 'git' 'clone' 'https://kieronmacknight:$github_password@github.com/355gts/CovidCore.git'

WORKDIR /data/CovidCore/Deployment/openshift_ansible/

RUN echo "$vault_password" > vault_password

ENV ANSIBLE_HOST_KEY_CHECKING=false
