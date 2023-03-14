package envoy.authz

import input.attributes.request.http as http_request

default allow = false

token = {"payload": payload} {
    [_, encoded] := split(http_request.headers.authorization, " ")
    [_, payload, _] := io.jwt.decode(encoded)
}

allow {
    action_allowed
    is_token_valid
}

is_token_valid {
  now := time.now_ns() / 1000000000
  token.payload.iat <= now
  now < token.payload.exp
}

action_allowed {
  http_request.method == "GET"
  glob.match("/WeatherForecast*", [], http_request.path)
}
