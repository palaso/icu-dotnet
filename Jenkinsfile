#!groovy
// Copyright (c) 2017 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

ansiColor('xterm') {
	timestamps {
		timeout(time: 60, unit: 'MINUTES') {
			properties([
				[$class: 'GithubProjectProperty', displayName: '', projectUrlStr: 'https://github.com/sillsdev/icu-dotnet'],
				// Add buildKind parameter
				parameters([choice(name: 'buildKind', choices: 'Continuous\nRelease',
					description: 'Is this a continuous (pre-release) or a release build?')]),
				// Trigger on GitHub push
				pipelineTriggers([[$class: 'GitHubPushTrigger']])
			])

			// Set default. This is only needed for the first build.
			def buildKindVar = params.buildKind ?: 'Continuous'
			def isPR = false

			try {
				isPR = env.BRANCH_NAME.startsWith("PR-") ? true : false
			}
			catch(err) {
				isPR = false
			}

			try {
				parallel('Windows build': {
					node('windows && supported') {
						def msbuild = tool 'msbuild15'
						def git = tool(name: 'Default', type: 'git')

						stage('Checkout Win') {
							checkout scm

							bat """
								"${git}" fetch origin --tags
								"""
						}

						stage('Build Win') {
							echo "Building icu.net"
							bat """
								"${msbuild}" /t:Build /property:Configuration=Release build/icu-dotnet.proj
								"""

							version = readFile "output/Release/version.txt"
							currentBuild.displayName = "${version}-${env.BUILD_NUMBER}"
						}

						stage('Tests Win') {
							echo "Running unit tests"
							bat """
								"${msbuild}" /t:TestOnly /property:Configuration=Release build/icu-dotnet.proj
								"""
						}

						stage('Upload nuget') {
							if (!isPR) {
								echo "Upload nuget package"
								withCredentials([string(credentialsId: 'nuget-api-key', variable: 'NuGetApiKey')]) {
									bat """
										build\\nuget.exe push -Source https://www.nuget.org/api/v2/package source\\NuGetBuild\\*.nupkg ${NuGetApiKey}
										"""
								}
								archiveArtifacts 'source/NuGetBuild/*.nupkg'
							}
						}

						nunit testResultsPattern: '**/TestResults.xml'
					}
				}, 'Linux': {
					node('linux64 && !packager && ubuntu && mono5') {
						stage('Checkout Linux') {
							checkout scm
						}

						stage('Build and Test Linux') {
							echo "Building icu.net"
							sh '''#!/bin/bash
ICUVER=$(icu-config --version|tr -d .|cut -c -2)

echo "Building for ICU $icu_ver"

MONO_PREFIX=/opt/mono5-sil
PATH="$MONO_PREFIX/bin:$PATH"
LD_LIBRARY_PATH="$MONO_PREFIX/lib:$LD_LIBRARY_PATH"
PKG_CONFIG_PATH="$MONO_PREFIX/lib/pkgconfig:$PKG_CONFIG_PATH"
MONO_GAC_PREFIX="$MONO_PREFIX:/usr"

export LD_LIBRARY_PATH PKG_CONFIG_PATH MONO_GAC_PREFIX

msbuild /t:Compile /property:Configuration=Release build/icu-dotnet.proj
msbuild /t:TestOnly /property:Configuration=Release build/icu-dotnet.proj
'''
						}

						nunit testResultsPattern: '**/TestResults.xml'
					}
				})
				currentBuild.result = "SUCCESS"
			} catch(error) {
				currentBuild.result = "FAILED"
			}
		}
	}
}