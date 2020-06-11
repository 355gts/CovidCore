FROM docker.io/aweiteka/playbook2image

USER root

RUN yum upgrade ansible -y && yum install sudo -y && yum clean -y all
RUN pip install --upgrade pip
RUN pip install --upgrade setuptools
RUN pip install openshift --ignore-installed

RUN chgrp -R 0 /opt && chmod -R g=u /opt

RUN chmod 777 /etc/passwd
RUN chmod 777 /dev/null

EXPOSE 22
USER 1001
