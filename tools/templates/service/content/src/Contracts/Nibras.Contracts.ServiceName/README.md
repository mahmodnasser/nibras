# Nibras.Contracts.ServiceName

The published contract of the ServiceName service: versioned events under `Events/V<n>/`, saga commands under
`Commands/V<n>/`, the gRPC surface under `Grpc/`, the error catalog under `ErrorCodes/`, and `RoutingKeys.cs`.

Rules (document 07, part 8): data only; references `Nibras.Contracts.Shared` and nothing else; a breaking change
publishes `V<n+1>` beside `V<n>`; a new routing key needs its Appendix E row first.

## Version retirement dates

None yet. When `V<n+1>` is added, the date on which `V<n>` retires is recorded here at the same time.
