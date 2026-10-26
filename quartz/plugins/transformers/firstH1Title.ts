import { QuartzTransformerPlugin } from "../types"
import { Root } from "mdast"
import { toString } from "mdast-util-to-string"

// [LLM Wiki patch] use the first "# heading" as the page title when no frontmatter title is set,
// and remove that heading from the body so the title is not rendered twice.
// must run right after FrontMatter()
export const FirstH1Title: QuartzTransformerPlugin = () => {
  return {
    name: "FirstH1Title",
    markdownPlugins() {
      return [
        () => {
          return (tree: Root, file) => {
            const frontmatter = file.data.frontmatter
            if (!frontmatter || frontmatter.title !== file.stem) return

            const index = tree.children.findIndex(
              (node) => node.type !== "yaml" && (node.type as string) !== "toml",
            )
            const first = tree.children[index]
            if (first?.type !== "heading" || first.depth !== 1) return

            const title = toString(first).trim()
            if (title === "") return

            frontmatter.title = title
            tree.children.splice(index, 1)
          }
        },
      ]
    },
  }
}
