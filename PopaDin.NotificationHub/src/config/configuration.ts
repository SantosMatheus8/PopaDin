export default () => ({
  port: parseInt(process.env.PORT ?? '3001', 10),
  mongodbUri: process.env.MONGODB_URI ?? 'mongodb://localhost:27017/popadin',
  serviceBus: {
    connectionString: process.env.SERVICEBUS_CONNECTION_STRING ?? '',
    queueName: process.env.SERVICEBUS_QUEUE_NAME ?? 'notifications',
  },
  jwt: {
    secret: process.env.JWT_SECRET ?? '',
  },
});
