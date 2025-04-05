This file explains how Visual Studio created the project.

The following tools were used to generate this project:
- create-vite

The following steps were used to generate this project:
- Create vue project with create-vite: `npm init --yes vue@latest rs.hmi.vueclient -- --eslint  --typescript `.
- Updating `vite.config.ts` with port.
- Add `shims-vue.d.ts` for basic types.
- Create project file (`rs.hmi.vueclient.esproj`).
- Create `launch.json` to enable debugging.
- Add project to solution.
- Write this file.
