FROM docker.io/aweiteka/playbook2image

USER root

RUN useradd -r covid -g wheel

RUN yum upgrade ansible -y
RUN pip install --upgrade pip
RUN pip install --upgrade setuptools
RUN pip install openshift --ignore-installed

RUN chown -Rf covid /opt
RUN chown covid /
RUN chown -Rf covid /usr/libexec/s2i/
RUN chmod 777 /etc/passwd
RUN chmod 777 /dev/null

EXPOSE 22
USER 1001
