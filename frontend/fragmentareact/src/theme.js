import { createSystem, defaultConfig, defineConfig } from "@chakra-ui/react"

const config = defineConfig({
  theme: {
    semanticTokens: {
      colors: {
        primary: {
          value: { base: "{colors.purple.500}", _dark: "{colors.purple.300}" },
        },
        background: {
          value: { base: "{colors.white}", _dark: "{colors.gray.900}" },
        },
        text: {
          value: { base: "{colors.gray.900}", _dark: "{colors.white}" },
        },
        success: {
          value: { base: "{colors.green.500}", _dark: "{colors.green.300}" },
        },
        warning: {
          value: { base: "{colors.yellow.500}", _dark: "{colors.yellow.300}" },
        },
        danger: {
          value: { base: "{colors.red.500}", _dark: "{colors.red.300}" },
        },
        info: {
          value: { base: "{colors.blue.500}", _dark: "{colors.blue.300}" },
        },
        border: {
          value: { base: "{colors.purple.300}", _dark: "{colors.purple.500}" },
        },
      },
    },
  },
})


export const system = createSystem(defaultConfig, config)