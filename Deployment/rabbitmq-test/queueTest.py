#!/usr/bin/env python

import pika

credentials = pika.PlainCredentials('service', 'Service123Service123')
parameters = pika.ConnectionParameters('ec2-3-8-181-169.eu-west-2.compute.amazonaws.com',
                                       5672,
                                       '/',
                                       credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()
channel.basic_publish(exchange='User', routing_key='CreateUser', body='{ "Firstname":"Python", "Surname":"Python", "DateOfBirth":"2018/06/05T13:14:15"}')
connection.close()