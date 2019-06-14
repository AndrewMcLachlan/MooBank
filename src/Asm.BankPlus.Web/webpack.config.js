const path = require("path");
const CircularDependencyPlugin = require('circular-dependency-plugin')

var rootPath = path.join(__dirname, "ClientApp");
var reactPath = path.join(rootPath, "src");

module.exports = (env, argv) => {

    return {
        entry: {
            dist: path.join(reactPath, "index.tsx")
        },
        watchOptions: {
            ignored: ["**/*.js", "node_modules"],
        },

        devtool: "source-map",

        resolve: {
            extensions: [".ts", ".tsx", ".js"]
        },

        output: {
            path: path.join(__dirname, "wwwroot", "js"),
            filename: argv.mode === "production" ? "[name].min.js" : "[name].js"
        },
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    exclude: /node_modules/,
                    enforce: 'pre',
                    use: [
                        {
                            loader: 'tslint-loader',
                            options: { /* Loader options go here */ }
                        }
                    ]
                },
                {
                    test: /\.tsx?$/,
                    exclude: /node_modules/,
                    use: [{
                        loader: "babel-loader"
                    },
                    {
                        loader: "ts-loader"
                    }],
                }
            ],
        },

        externals: {
            "react": "React",
            "react-dom": "ReactDOM",
            "redux": "Redux",
            "react-redux": "ReactRedux",
            "redux-thunk": "ReduxThunk",
            "react-router": "ReactRouter",
            "react-router-dom": "ReactRouterDOM",
            "msal": "Msal"
        },
        plugins: [
            new CircularDependencyPlugin({
                // exclude detection of files based on a RegExp
                exclude: /node_modules/,
                // add errors to webpack instead of warnings
                failOnError: true,
                // set the current working directory for displaying module paths
                cwd: process.cwd(),
            })
        ]
    };
};