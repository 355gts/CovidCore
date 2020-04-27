#!/usr/bin/env python

import pika, json

credentials = pika.PlainCredentials('service', 'Service123Service123')
parameters = pika.ConnectionParameters('ec2-3-8-181-169.eu-west-2.compute.amazonaws.com',
                                       5672,
                                       '/',
                                       credentials)
connection = pika.BlockingConnection(parameters)
channel = connection.channel()
channel.basic_publish(exchange='User', routing_key='CreateUser', body='{ "Firstname":"Python", "Surname":"Python", "DateOfBirth":"2018/06/05T13:14:15"}')
for method_frame, properties, body in channel.consume('User'):

    # Acknowledge the message
    channel.basic_ack(method_frame.delivery_tag)
    messageBody = json.loads(body)

    if messageBody["Firstname"] != "Python":
    	raise Exception("Incorrect message details {0}".format(messageBody))
    	break
    else:
    	break

connection.close()