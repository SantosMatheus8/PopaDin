export default () => ({
  port: parseInt(process.env.PORT ?? '3002', 10),
  mongodbUri: process.env.MONGODB_URI ?? 'mongodb://localhost:27017/popadin',
  jwt: {
    secret: process.env.JWT_SECRET ?? '',
  },
  cron: {
    schedule: process.env.CRON_SCHEDULE ?? '0 */6 * * *',
  },
});
