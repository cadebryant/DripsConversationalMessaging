const { env } = require('process');

const target = env["services__dripsconversationalmessaging-server__https__0"] ?? 'https://localhost:7051';

const PROXY_CONFIG = [
  {
    context: ["/api"],
    target,
    secure: false
  }
]

module.exports = PROXY_CONFIG;
